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

## Static ownership findings — pinned Graveyard Keeper 1.407 decompile

**Status:** verified static fact.

Primary source: `Kupie/GYK_DECOMP@6abf79199d92482af1c7573870dd9a20ec2270b9`.

### Vanilla row construction

`ItemDefinition.GetTooltipData(Item, bool)` owns the standard item's crafting-location text construction:

1. `GetItemDetails()` supplies `ItemDetails.crafts_in`.
2. The method preserves the list order and localizes each `ObjectDefinition.id` through `GJL.L(id)`.
3. It uses the localized comma token `GJL.L(",")` when available.
4. If at least one location exists, it appends one final `BubbleWidgetTextData` row:
   `GJL.L("crafted_at") + " " + joined localized station names`.
5. The row uses `TinyDescription`, centered alignment, and the standard native bubble renderer.

`ItemDefinition.GetTooltipDataCraftAt(Item)` duplicates the same craft-location formatting as a dedicated one-row helper.

### Canonical data source and ordering

`ItemDefinition.GetItemDetails()` caches:

`crafts_in = GameBalance.me.GetItemCraftsIn(this.id)`.

`GameBalance.CreateCraftsCache()` builds the backing item-to-station cache by iterating native `craft_data`, then each recipe's `craft_in` list. It:

- excludes `grave_ground`;
- excludes hidden recipes and recipes marked `dont_show_in_hint`;
- resolves each craft-in ID to its native `ObjectDefinition`;
- adds a station only once per output item;
- therefore preserves the first native encounter order established by `craft_data -> craft_in`.

`GetItemCraftsIn` returns that cached `List<ObjectDefinition>` (with the game's normal quality/base-ID fallback).

The compact formatter must consume this same ordered native list rather than reconstructing recipe availability independently.

### Known consumers / blast radius

Pinned source contains two direct callers of `ItemDefinition.GetTooltipData(...)`:

- `BaseItemCellGUI` — standard item-cell tooltip path, with `full_detail=true`;
- `TechUnlock` — technology unlock presentation, with `full_detail=false`.

`TechUnlock` is also the only direct caller found for `GetTooltipDataCraftAt(...)`, used in its special multi-quality prayer-output branch.

Therefore a patch on `GetTooltipData` is not inventory-only by definition. A patch on the dedicated `GetTooltipDataCraftAt` affects a technology-unlock surface. Production scope must account for those callers explicitly.

### Final text consumer

`BubbleWidgetText.Draw(BubbleWidgetTextData)` applies the row's style/alignment/font and then assigns `UILabel.text = data.text`. No later text transformation is visible in that verified draw step; enclosing `WidgetsBubbleGUI` behavior owns sizing/repositioning rather than the string contents.

### Remaining blocker: family/tier identity

`ObjectDefinition` does not expose an obvious generic `tier`, `upgrade_parent`, or station-family field in the pinned class definition. Public source contains examples of tier-like IDs such as `zombie_garden_desk_1/2/3` and `refugee_camp_garden_bed_1/2/3`, but examples are not sufficient to define a whole-game grouping rule.

The production gate therefore remains **BLOCKED** only on the semantic grouping/taxonomy question and its representative acceptance set; owner, source, ordering, direct callers, and final text consumer are now statically established.

## Research-method checkpoint — station family/tier taxonomy

**Question:** determine the complete native crafting-station ID set used by `GetItemCraftsIn` in the live 1.407 balance, identify which IDs are genuine tier variants of one station family, and inspect available native metadata strongly enough to choose a generic grouping rule without hard-coded display strings.

**Existing path checked:** owning-project docs, shared Graveyard Keeper research, pinned decompiled source, and public source examples. Static code proves where the data comes from but does not include the loaded balance rows needed to prove the family taxonomy exhaustively.

**Probe justification:** one read-only, one-shot runtime dump after `MainGame.OnGameStartedPlaying()` can enumerate the exact native `GetItemCraftsIn` lists, station IDs, active localized names, order, craft preset/subtype, object groups and related station metadata. This is lower-risk and more complete than manually inspecting many item tooltips or inferring semantics from a few public examples. It requires no save mutation, no per-frame work, and no production patch.

**Selected research path:** build a separate research-only BepInEx DLL that logs this raw taxonomy once per loaded game session. Do not mutate production source while this gate is blocked.
