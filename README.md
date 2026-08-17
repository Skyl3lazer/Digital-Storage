# Digital Storage

A RimWorld mod that bridges the gap between low-tier information storage (bookshelves) and larger modded solutions. It adds a line of industrial-tier shelves that store far more books than a bookcase but still grant reading bonuses.

If the buildings lose power, colonists can no longer add or remove items, and they stop providing any reading bonus.

## Research

| Project | Cost | Prerequisites | Enables |
|---|---|---|---|
| **Digital Storage** | 700 | Electricity (also requires *Personal Computer* if [Generally More Research](https://steamcommunity.com/sharedfiles/filedetails/?id=3613068789) is installed) | Microfilm Reader, Digitized Index |
| **Databases** | 1200 | Digital Storage | PawnPedia Terminal, Digital Ocean |

## Buildings

All digital storages hold books just as a vanilla bookcase does. Advanced ones can also increase reading bonus. They only function while powered.

| Building | Size | Capacity | Power | Reading bonus | Materials |
|---|---|---|---|---|---|
| **Microfilm Reader** | 1x1 | 40 | 25 W | standard | 20 stuff (metallic/woody/stony), 25 steel, 1 component |
| **Digitized Index** | 1x2 | 80 | 50 W | standard | 50 stuff (metallic/woody/stony), 50 steel, 2 components |
| **PawnPedia Terminal** | 1x1 | 80 | 50 W | 1.5x (maxes a room's reading bonus when full) | 50 stuff (metallic/stony), 100 steel, 1 advanced component |
| **Digital Ocean** | 1x2 | 200 | 150 W | standard | 100 stuff (metallic/stony), 100 steel, 1 advanced component |

## Dependencies

- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- [Adaptive Storage Framework](https://steamcommunity.com/sharedfiles/filedetails/?id=3033901359)
- (Optional) [Generally More Research](https://steamcommunity.com/sharedfiles/filedetails/?id=3613068789) gates the research behind *Personal Computer*.
- (Optional) [Research Papers](https://steamcommunity.com/sharedfiles/filedetails/?id=3492739424) - Makes the Research Server into a digital storage building, restoring the per-book room reading bonus to contained items.

## Incompatibilities

Any mods that modify the reading bonus calculations of a room would clash with this mod, as I reimplement the reading bonus calculation to allow the shelves to still give normal per-book (or enhanced per-book) bonuses. Without this patch then each book would be worth signifigantly less reading bonus while in the modded shelves.

## Supported versions

- RimWorld 1.6

## Check Out My Other Mods!

[Apparel Policy Builder](https://steamcommunity.com/sharedfiles/filedetails/?id=3761873351) - Build apparel policies from rules over armor, insulation, coverage, and any other stat instead of a flat item list.

[Break Timer](https://steamcommunity.com/sharedfiles/filedetails/?id=3732890624) - See what breaks your pawns are at risk of, and find out when they'll get over it.

[Pipes for Medieval Overhaul](https://steamcommunity.com/sharedfiles/filedetails/?id=3725970365) - Adds DBH water pipes to some MO and MO mod objects.

## Thanks to

* CoMiGo, for the Rimworld Sprite Constructor figma that I used to make the graphics for this mod. Check out their stuff here: https://comigo.itch.io/

### AI Disclosure:

[![AI-DECLARATION: copilot](https://img.shields.io/badge/䷼%20AI--DECLARATION-copilot-fee2e2?labelColor=fee2e2)](https://ai-declaration.md)

*This level comes from the use of AI in testing and deployment tasks, see [AI-DECLARATION.md](AI-DECLARATION.md)*

This mod was partially developed with the assistance of AI tools, used by an actual programmer who understands the mod and any code it produced.