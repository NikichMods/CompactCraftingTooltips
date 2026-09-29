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


## Runtime taxonomy findings — research handoff r2

**Status:** accepted runtime evidence for Graveyard Keeper 1.407, Russian localization.

Exact research identity:
- source branch: `research/crafting-location-taxonomy`
- source SHA: `94088d20dcb507e0f7319db384321336346e4a4e`
- CI run: `36571564735`
- artifact ID: `11033843702`
- handed DLL: `CompactCraftingTooltips-Research-r2.dll`
- DLL SHA-256: `9b58d535af23ae1fc11cb0f39a06c44cdbc4fe6517ce418b0756f4f3c6345217`
- returned runtime: Graveyard Keeper 1.407, BepInEx 5.4.23.5, language `ru`.

The probe completed successfully with:
- `items_with_locations=702`;
- `items_with_multiple_locations=511`;
- `unique_stations=79`;
- `raw_links=1402`;
- `suffix_candidate_families=11`;
- no save or UI mutation.

### What the live data proves

1. The reported farm case is exactly present in canonical native order:
   - `zombie_garden_desk_0/1/2` -> `Зомби-ферма`, `Зомби-ферма II`, `Зомби-ферма III`;
   - `refugee_camp_garden_bed_1/2/3` -> `Грядка`, `Грядка II`, `Грядка III`.
2. Higher-quality crop variants can legitimately expose only a suffix of a family, for example tiers II+III or only III. The formatter must compact only the locations actually present and must not synthesize unavailable tiers.
3. Numeric station-ID suffixes alone are not semantically safe:
   - `mf_anvil_1/2/3` localize as `Деревянная наковальня`, `Наковальня`, `Наковальня II`;
   - `mf_alchemy_craft_01/02/03` localize as `Алхимический верстак (I)`, `Алхимический стол`, `Алхимический стол II`.
   Therefore "same numeric ID stem" is only a candidate-family signal, not sufficient proof to merge all members into one visible family.
4. Conversely, live data contains safe presentation pairs/runs in which canonical IDs share a structural family and active localized names share one visible base plus Roman tier suffixes. Examples include furnaces, preparation tables, workbenches, zombie farms, refugee garden beds, desks, refugee cooking tables, embalming tables and others.
5. A fail-closed formatter can therefore use **both** canonical ID-family continuity and active localized-name compatibility. It may group only a contiguous, order-preserving run whose visible names prove the same base/tier sequence; otherwise it leaves the vanilla entries unchanged.
6. This avoids a manual Russian station-name table and avoids claiming that every numeric suffix is a tier. It also degrades safely in another localization: if the active localized names do not expose a compatible tier pattern, the affected entries remain vanilla rather than being guessed.

One observed safe false-negative is acceptable for the initial implementation: `mf_distcube_2_clay` / `mf_distcube_2_cuprum` display as `Дистилляционный куб` / `Дистилляционный куб II` but do not share the generic terminal-numeric ID family. The initial formatter should not add a hard-coded exception merely to compact that pair.

## Solution-space checkpoint — initial production formatter

Useful solution families considered:

1. **Numeric ID suffix only.** Rejected: live anvil/alchemy data proves false semantic merges.
2. **Manual station-family table.** Not selected: it would be more brittle and broader to maintain, and the current product goal can be met from native IDs plus active localization without a per-language display table.
3. **Localized display text only.** Not selected by itself: two unrelated canonical stations could theoretically localize to similar visible names.
4. **Canonical ID-family guard + localized semantic/tier compatibility, fail closed.** Selected. This uses native station identity to constrain grouping and native active-language text to prove that the visible entries form one tier family.

The selected formatter:
- consumes the same ordered `GetItemCraftsIn` list as vanilla;
- never adds, removes or reorders underlying locations;
- compacts only contiguous compatible runs;
- preserves partial families such as `II, III`;
- leaves reversed/nonsequential/renamed/mismatched runs unchanged;
- does not mutate recipes, station definitions, localization resources or item data.

## Production evidence gate — compact crafting-location behavior

**Observable property:** standard full-detail item tooltips compact repeated compatible station tiers while representing exactly the same ordered native crafting locations.

**Canonical owner:** `ItemDefinition.GetTooltipData(Item, bool)` constructs the row from `GetItemDetails().crafts_in`, whose canonical source is `GameBalance.GetItemCraftsIn(item_id)`.

**Final writer / consumer:** the returned `BubbleWidgetTextData.text` is consumed by `BubbleWidgetText.Draw`, which assigns the final label text. The enclosing `WidgetsBubbleGUI` owns geometry only.

**Chosen seam / blast radius:** postfix `ItemDefinition.GetTooltipData`, guarded to `full_detail == true`. In the pinned 1.407 callers this targets the standard `BaseItemCellGUI` item-tooltip path and deliberately leaves the `TechUnlock` `full_detail=false` path and `GetTooltipDataCraftAt` helper unchanged.

**Preserved invariants:** native location membership and order; recipes/availability; progression; research rewards; station behavior; item mechanics/data; trading data; unrelated tooltip rows; technology-unlock crafting-location presentation; active-language station names; no per-frame work.

**Mechanical acceptance:** formatter contract tests must cover full I/II/III families, II/III subsets, renamed members, reversed order, mixed families, and no-change fallbacks; production row replacement must occur only when the exact vanilla crafting row is found and at least one safe grouping was produced.

**Runtime acceptance:** real-game inspection of representative full-detail item tooltips must confirm readability, wrapping, clipping, punctuation and that unrelated tooltip sections remain unchanged.

**Gate state: READY.**

Production implementation may now proceed on a dev branch. Runtime visual acceptance is still required before promotion to stable `main`.
