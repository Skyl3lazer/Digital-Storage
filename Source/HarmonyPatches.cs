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
        // Read from vanilla values at startup
        private static readonly float MaxEnhancement;
        private static readonly List<float> CellFilledFactor;

        static RoomStatWorker_ReadingBonus_Patch()
        {
            FieldInfo maxField = AccessTools.Field(typeof(RoomStatWorker_ReadingBonus), "MaxEnhancement");
            FieldInfo factorField = AccessTools.Field(typeof(RoomStatWorker_ReadingBonus), "CellFilledFactor");
            MaxEnhancement = maxField != null ? (float)maxField.GetValue(null) : 0.2f;
            CellFilledFactor = factorField?.GetValue(null) as List<float> ?? new List<float> { 0.04f, 0.02f, 0.01f, 0.005f };
            if (maxField == null || factorField == null)
            {
                Log.Warning("[Digital Storage] Could not read vanilla ReadingBonus constants via reflection; using last-known defaults. Reading bonus may be inaccurate for this game version.");
            }
        }

        static bool Prefix(Room room, ref float __result)
        {
            float filledCells = 0f;

            foreach (Building_Bookcase bookcase in room.ContainedThings<Building_Bookcase>())
            {
                foreach (float cell in bookcase.CellsFilledPercentage)
                {
                    filledCells += cell;
                }
            }

            foreach (Building_Storage shelf in room.ContainedThings<Building_Storage>())
            {
                DigitalShelfExtension ext = shelf.def.GetModExtension<DigitalShelfExtension>();
                if (ext == null || !PowerCheck.Powered(shelf) || shelf.slotGroup == null)
                {
                    continue;
                }
                int books = 0;
                foreach (Thing thing in shelf.slotGroup.HeldThings)
                {
                    if (thing is Book)
                    {
                        books++;
                    }
                }
                filledCells += books * ext.readingBonusPerBook;
            }

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
