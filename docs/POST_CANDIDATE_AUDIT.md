# Post-candidate compaction audit

Target candidate visually reviewed by the user: Compact Crafting Tooltips 0.1.0.

## Accepted visual observations

Representative English screenshots show the intended existing compaction behaving correctly for:
- Furnace I, II, III;
- Zombie Vineyard I, II, III;
- Zombie Farm I, II, III + Bed I, II, III on separate compact lines;
- Wooden anvil kept distinct while Anvil / Anvil II compact to `Anvil I, II`.

The user also reports that the same categories behave equivalently in Russian. The current candidate is therefore visually close to the desired product, but is not promoted because the follow-up audit below may change behavior.

## Runtime catalogue audit

The accepted r2 runtime catalogue exposes 79 unique crafting-station definitions and 511 items with multiple crafting locations. Reviewing all unique multi-location sequences identified three material follow-up cases and several deliberate non-cases.

### 1. Sweet Home cooking table pair

Canonical IDs:
- `cooking_table` -> vanilla display `Кухонный стол`
- `cooking_table_2` -> vanilla display `Обновленный кухонный стол`

External game documentation describes the second as the updated appearance of the Sweet Home cooking table rather than a separate higher-function workstation.

User-proposed presentation:
- `Cooking table I, II` / equivalent active-language base name + Roman tiers.

Important collision:
- the Game of Crone refugee camp has a different pair:
  - `refugee_camp_cooking_table`
  - `refugee_camp_cooking_table_2`
- those already display as `Cooking table` / `Cooking table II` (Russian likewise) and the current generic formatter compacts them naturally.
- some items, e.g. `dough`, contain **both** the Sweet Home pair and the refugee-camp pair, plus `tavern_kitchen`.
- blindly normalizing the Sweet Home pair to `Cooking table I, II` would therefore produce two visually identical compact families in one tooltip and erase the only remaining visible distinction.

**Observable property:** optionally normalize the Sweet Home `cooking_table -> cooking_table_2` pair without making tooltips containing the refugee-camp pair more ambiguous.

**Canonical owner:** same verified `ItemDefinition.GetTooltipData -> GameBalance.GetItemCraftsIn` path.

**Final writer / consumer:** same verified `BubbleWidgetTextData.text -> BubbleWidgetText.Draw` path.

**Blast radius:** full-detail item tooltips only; many food/ingredient entries contain the home pair, and a subset also contains the refugee pair and/or tavern kitchen.

**Preserved invariants:** exact native location membership/order; active-language base text; professional/refugee stations remain distinct semantic families.

**Acceptance evidence:** contract cases for home pair alone, home pair + tavern kitchen, home pair + refugee pair, and home + refugee + tavern; English and Russian visual check.

**Gate state: BLOCKED** pending the product decision for collision policy. No production mutation yet.

### 2. Professional Kitchen / Professional Oven

Canonical IDs:
- `tavern_kitchen`
- `tavern_oven`

Pinned 1.407 source has dedicated branches for these IDs that route produced food toward the tavern/Barman path. They are not the refugee-camp cooking stations.

External game documentation places both in the Talking Skull Cellar. The refugee camp uses the separate `refugee_camp_cooking_table*` family.

Conclusion:
- do **not** reinterpret `Professional Kitchen` as `Cooking table III`;
- do **not** reinterpret `Professional Oven` as `Oven II`;
- they are separate-location, separate-output-semantics stations and should remain visibly distinct.

### 3. Distillation Cube pair

Canonical IDs:
- `mf_distcube_2_clay` -> `Дистилляционный куб`
- `mf_distcube_2_cuprum` -> `Дистилляционный куб II`

The pair is a genuine visible I/II workstation family, but the generic terminal-number ID rule cannot detect it because the IDs end in material names. Accepted runtime examples show both consecutively for alchemy extract items.

Potential presentation:
- `Distillation cube I, II` using the active localized base from the first canonical station.

This requires a narrow canonical-ID family alias rather than a display-string table.

**Observable property:** compact this exact native pair without broadening family inference.

**Canonical owner/final writer/blast radius:** same verified full-detail tooltip seam; affected items are the extract/alchemy entries whose native list contains the pair.

**Preserved invariants:** exact two native stations and their order; localization base from native `GJL.L`; no recipe/station mutation.

**Acceptance evidence:** contract test for the exact pair plus runtime visual check on one representative extract item.

**Gate state: BLOCKED** pending user approval to include this follow-up behavior.

### 4. Duplicate Zombie Mine display entry

Accepted runtime data contains:
- `zombie_mine_fence_front`
- `zombie_mine_fence_left_front`

Both localize to the same visible name, `Зомби-шахта`, and occur consecutively for the stone item before `steep_stone`. Vanilla therefore exposes two identical visible station names even though they are different internal object IDs.

Potential presentation:
- collapse only this proven canonical duplicate pair to one visible `Zombie Mine` entry.

Do not introduce generic global de-duplication yet: the catalogue also contains distinct station IDs that legitimately share a display name, most notably the Sweet Home and refugee-camp cooking tables.

**Gate state: BLOCKED** pending user approval and final confirmation that this exact pair should be treated as one player-facing location.

## Deliberate non-cases

### Wooden anvil / Anvil / Anvil II

Keep the wooden anvil distinct. It is a separate primitive workstation. The current output `Wooden anvil, Anvil I, II` is the desired interpretation; do not relabel all three as one artificial I/II/III family.

### Alchemy workbench anomaly

The live catalogue contains the unusual raw sequence for `goo`:
- `mf_alchemy_craft_01` -> `Алхимический верстак (I)`
- `mf_alchemy_craft_02` -> `Алхимический стол`
- `mf_alchemy_craft_03` -> `Алхимический стол II`

The current formatter deliberately does not force all three into one family; it may compact only the visibly compatible latter pair. Do not add a broad alias until a concrete UX problem is observed.

### Home Oven / Professional Oven

Keep distinct. The professional oven belongs to the tavern path and has special output semantics.

### Sweet Home / refugee-camp cooking stations

Never merge these two canonical families merely because localization can produce the same base display name.


## User decisions and follow-up verification — 2026-09-29

The user approved these presentation changes:
- Sweet Home `cooking_table -> cooking_table_2` may display as one `Cooking table I, II` family, provided collision with the refugee-camp cooking-table family is handled safely.
- `mf_distcube_2_clay -> mf_distcube_2_cuprum` may display as `Distillation cube I, II`.
- the actual modern Alchemy Workbench tier pair may display as `Alchemy workbench I, II`.
- `Professional Kitchen`, `Professional Oven`, and `Wooden Anvil` remain distinct and unmodified.

### Alchemy Workbench identity

Fresh cross-check:
- current community/mod source used for Graveyard Keeper identifies `mf_alchemy_craft_02` as Alchemy Workbench tier I and `mf_alchemy_craft_03` as tier II;
- the official wiki likewise documents exactly two player workbenches, tier I (two ingredients) and tier II (three ingredients);
- `mf_alchemy_craft_01` is a separate legacy/stale balance definition whose localization still says `Alchemy workbench (Tier I)`; it is not the active tier-I station used by modern alchemy UI code.

Production rule:
- normalize only the exact canonical pair `mf_alchemy_craft_02 -> mf_alchemy_craft_03` to active-localization base + `I, II`;
- do not merge `mf_alchemy_craft_01` into that family merely because its stale display text contains `Tier I`.

**Gate state: READY** for the exact 02/03 pair.

### Zombie Mine / quarry verification

Pinned 1.407 host code distinguishes three zombie mining interaction object IDs:

- `mine_zombie_bench` — iron production (`mine_zombie_bench_iron_production`);
- `zombie_mine_fence_left_front` — stone production;
- `zombie_mine_fence_front` — position-dependent: one placed instance starts stone production, two other placed instances start marble production.

Therefore the left/right/front object IDs are **interaction points / placed instances, not workstation tiers**.

The accepted runtime crafting-location catalogue exposes:
- stone: `zombie_mine_fence_front -> zombie_mine_fence_left_front -> steep_stone`, both zombie IDs localizing to the same player-facing `Zombie Mine`;
- marble: `zombie_mine_fence_front -> steep_marble_2`, so the multiple marble-side placed instances are already deduplicated by the vanilla `GetItemCraftsIn` object-definition cache because they share the same canonical object ID.

Production consequence:
- for the exact stone duplicate pair, collapse the two proven zombie-mine IDs to **one** player-facing `Zombie Mine` entry, preserving its first native position;
- do **not** relabel the pair as `Zombie Mine I, II`: the host evidence proves they are not tiers;
- no special marble-side duplicate rule is needed because vanilla already exposes only one `zombie_mine_fence_front` definition for marble;
- `steep_stone` (Stone deposit) and `steep_marble_2` (Marble quarry) remain separate non-zombie crafting locations and must not be merged with Zombie Mine.

**Gate state: READY** for the exact stone duplicate collapse. No generic same-name deduplication is approved.

### Sweet Home Cooking Table collision policy

Approved production policy:
- exact home pair `cooking_table -> cooking_table_2` may normalize to `Cooking table I, II`;
- if the same native list also contains `refugee_camp_cooking_table` and/or `refugee_camp_cooking_table_2`, do not normalize the home pair in a way that produces two indistinguishable `Cooking table I, II` families;
- the refugee pair remains its own canonical family and may continue to compact generically;
- `tavern_kitchen` remains Professional Kitchen.

**Gate state: READY** with fail-closed collision guard.

### Distillation Cube

Approved exact alias family:
- `mf_distcube_2_clay`
- `mf_distcube_2_cuprum`

Use the active localized base from the first member and display `I, II`. No broader material-suffix inference is approved.

**Gate state: READY**.

No production source has been mutated for these follow-up behaviors yet.
