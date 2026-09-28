# Verified Game Data

Target: **Graveyard Keeper 1.407 (PC)**.

This document is the canonical project ledger for host/runtime facts that production decisions in Compact Crafting Tooltips may rely on. Reusable cross-project facts should also be promoted to `NikichMods/GraveyardKeeperResearch`.

## Evidence baseline

Established shared facts already available before project-specific research:

- `ItemDefinition.GetTooltipData(Item item = null, bool full_detail = true)` returns the standard `List<BubbleWidgetData>` used by ordinary item cells.
- The standard item-tooltip family is rendered through `WidgetsBubbleGUI` child bubble widgets.
- Existing shared evidence about standard item-tooltip child alignment and sizing applies only to the inspected standard item-tooltip family and does not by itself identify the owner of the vanilla crafting-location block.

Canonical source: `NikichMods/GraveyardKeeperResearch/docs/RESEARCH_INDEX.md`, especially `docs/CRAFTING_INVENTORY_AND_TRADING.md` and `docs/GAME_INTERNALS.md`.

The user's supplied Graveyard Keeper 1.407 screenshot demonstrates the product problem in Russian: the `Создаётся на:` block can enumerate repeated tier variants such as the Zombie Farm and Garden Bed families, producing several visually dense wrapped entries.

## Product invariant

The requested change is presentation-only: represent the same crafting locations more compactly, grouping tier variants of one station family while preserving the underlying set of locations.

Conceptual desired presentation:
- `Zombie Farm I, II, III`
- `Garden Bed I, II, III`

The exact punctuation, line layout, tier notation and localization mechanism remain product/layout details to validate against the game's actual data and runtime rendering.

## Initial production evidence gate

**Observable property:** the item tooltip's crafting-location section should display repeated tier variants as compact grouped station-family entries without losing or adding crafting locations.

**Canonical owner:** **BLOCKED** — the standard tooltip entry point is known, but the exact code/data owner that builds the vanilla crafting-location section has not yet been established.

**Final writer / consumer / commit point:** **BLOCKED** — the final relevant formatter/data row and any downstream overwrite/layout behavior for this section must be identified.

**Blast radius:** **BLOCKED** — enumerate all callers/surfaces affected by the chosen formatter/hook before changing shared code.

**Preserved invariants:** recipes, availability, progression, research rewards, station behavior, item mechanics/data, trading data, unrelated tooltip sections, vanilla location membership, localization behavior, and vanilla ordering unless separately approved.

**Acceptance evidence:** mechanical checks must prove exact grouping/membership/order semantics; real-game visual acceptance must cover readability, wrapping, clipping and localization appearance on representative items with long location lists.

**Gate state: BLOCKED.**

Production source must not be mutated for this behavior until the blocked ownership/final-writer/blast-radius questions are closed.

## Research questions

Close these in order where practical:

1. Which method constructs the vanilla crafting-location tooltip row(s)?
2. Which native data structure supplies the location/station entries, and what establishes their order?
3. Does native data expose station-family/tier identity independently of localized display strings?
4. How are station names localized, and can grouping preserve active-language behavior without a manual language table?
5. Is the location block one `BubbleWidgetData` row, several rows, or another structure by the time `GetTooltipData` returns?
6. What other UI surfaces or callers would be affected by patching the narrowest candidate seam?
7. Which downstream `WidgetsBubbleGUI` sizing/wrapping behavior matters for one grouped line per station family?

Do not assume Roman-numeral parsing, prefix stripping, hard-coded station IDs, or a specific Harmony patch point until evidence supports it.
