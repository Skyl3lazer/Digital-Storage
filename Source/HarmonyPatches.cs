using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DigitalStorage
{
    [StaticConstructorOnStartup]
    public static class DigitalStorageMod
    {
        static DigitalStorageMod()
        {
            new Harmony("Skyl3lazer.DigitalStorage").PatchAll();
        }
    }

    internal static class PowerCheck
    {
        public static bool Powered(Thing thing)
        {
            CompPowerTrader power = thing.TryGetComp<CompPowerTrader>();
            return power == null || power.PowerOn;
        }
    }

    // Blocks pawns from hauling items into an unpowered digital shelf.
    [HarmonyPatch(typeof(Building_Storage), nameof(Building_Storage.Accepts))]
    internal static class Building_Storage_Accepts_Patch
    {
        static void Postfix(Building_Storage __instance, ref bool __result)
        {
            if (!__result || !__instance.def.HasModExtension<DigitalShelfExtension>())
            {
                return;
            }
            if (!PowerCheck.Powered(__instance))
            {
                __result = false;
            }
        }
    }

    // Blocks pawns from taking (hauling away or reading) items held in an unpowered
    // digital shelf. Guarded by the Book type check so the hot forbid path stays cheap.
    [HarmonyPatch(typeof(ForbidUtility), nameof(ForbidUtility.IsForbidden), new[] { typeof(Thing), typeof(Pawn) })]
    internal static class ForbidUtility_IsForbidden_Patch
    {
        static void Postfix(Thing t, ref bool __result)
        {
            if (__result || !(t is Book) || !t.Spawned)
            {
                return;
            }
            Building storer = t.GetSlotGroup()?.parent as Building;
            if (storer != null && storer.def.HasModExtension<DigitalShelfExtension>() && !PowerCheck.Powered(storer))
            {
                __result = true;
            }
        }
    }

    // Recomputes the room reading bonus, folding powered digital shelves into the same
    // diminishing-returns curve vanilla uses for bookcases
    [HarmonyPatch(typeof(RoomStatWorker_ReadingBonus), nameof(RoomStatWorker_ReadingBonus.GetScore))]
    internal static class RoomStatWorker_ReadingBonus_Patch
    {
        private readonly struct DigitalShelfKind
        {
            public readonly ThingDef def;
            public readonly float perBook;
            public DigitalShelfKind(ThingDef def, float perBook) { this.def = def; this.perBook = perBook; }
        }

        private static readonly float MaxEnhancement;
        private static readonly List<float> CellFilledFactor;
        private static readonly List<ThingDef> PlainBookcaseDefs = new List<ThingDef>();
        private static readonly List<DigitalShelfKind> DigitalShelfDefs = new List<DigitalShelfKind>();

        static RoomStatWorker_ReadingBonus_Patch()
        {
            // Reflect to get the vanilla attributes in case they change
            FieldInfo maxField = AccessTools.Field(typeof(RoomStatWorker_ReadingBonus), "MaxEnhancement");
            FieldInfo factorField = AccessTools.Field(typeof(RoomStatWorker_ReadingBonus), "CellFilledFactor");
            MaxEnhancement = maxField != null ? (float)maxField.GetValue(null) : 0.2f;
            CellFilledFactor = factorField?.GetValue(null) as List<float> ?? new List<float> { 0.04f, 0.02f, 0.01f, 0.005f };
            if (maxField == null || factorField == null)
            {
                Log.Warning("[Digital Storage] Could not read vanilla ReadingBonus constants via reflection; using last-known defaults. Reading bonus may be inaccurate for this game version.");
            }

            // Prepopulate defs so we don't have to walk every item in the rooms
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                DigitalShelfExtension ext = def.GetModExtension<DigitalShelfExtension>();
                if (ext != null)
                {
                    DigitalShelfDefs.Add(new DigitalShelfKind(def, ext.readingBonusPerBook));
                }
                else if (def.thingClass != null && typeof(Building_Bookcase).IsAssignableFrom(def.thingClass))
                {
                    PlainBookcaseDefs.Add(def);
                }
            }
        }

        static bool Prefix(Room room, ref float __result)
        {
            float filledCells = 0f;

            // Untagged bookcases (vanilla + modded)
            foreach (ThingDef def in PlainBookcaseDefs)
            {
                foreach (Thing thing in room.ContainedThings(def))
                {
                    if (thing is Building_Bookcase bookcase)
                    {
                        foreach (float cell in bookcase.CellsFilledPercentage)
                        {
                            filledCells += cell;
                        }
                    }
                }
            }

            // Digital shelves
            foreach (DigitalShelfKind shelf in DigitalShelfDefs)
            {
                foreach (Thing thing in room.ContainedThings(shelf.def))
                {
                    if (!PowerCheck.Powered(thing))
                    {
                        continue;
                    }
                    int books;
                    if (thing is Building_Storage storage)
                    {
                        books = storage.slotGroup?.HeldThingsCount ?? 0;
                    }
                    else if (thing is Building_Bookcase bookcase)
                    {
                        books = bookcase.HeldBooks.Count;
                    }
                    else
                    {
                        continue;
                    }
                    filledCells += books * shelf.perBook;
                }
            }

            // Vanilla logic
            float score = 0f;
            int step = 0;
            while (filledCells > 0f && score < MaxEnhancement)
            {
                float chunk = (filledCells >= 1f) ? 1f : filledCells;
                filledCells -= chunk;
                score += chunk * CellFilledFactor[Mathf.Min(step++, CellFilledFactor.Count - 1)];
            }

            __result = 1f + Mathf.Min(score, MaxEnhancement);
            return false;
        }
    }
}
