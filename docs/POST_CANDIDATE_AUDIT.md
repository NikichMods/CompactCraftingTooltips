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
