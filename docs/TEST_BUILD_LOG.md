# Test / Research Build Log

## Crafting-location taxonomy probe

Purpose: close the remaining production evidence-gate question for Compact Crafting Tooltips: identify the complete live Graveyard Keeper 1.407 crafting-station taxonomy used by `GameBalance.GetItemCraftsIn` and determine whether tiered station families can be grouped generically without hard-coded display strings or an unsafe numeric-suffix assumption.

### Research handoff r1 — rejected by BepInEx before execution

Status: **failed handoff; frozen**

Artifact:
- identity: `CompactCraftingTooltips-Research.dll`
- plugin/research version metadata: `0.0.0-research`
- source branch: `research/crafting-location-taxonomy`
- exact source SHA: `a56bec010603b5224ad42cb36a244a90abfc00a9`
- GitHub Actions run: `36471545218`
- GitHub Actions artifact ID: `10992280727`
- DLL SHA-256: `372d3dc600f02f87493dd9b6f0419962d6e624658747c7cc7bcf19d2f71d763e`
- CI result: **success**
- handoff date: 2026-09-28

Returned runtime evidence, 2026-09-29:
- Graveyard Keeper started as version 1.407 under BepInEx 5.4.23.5.
- Chainloader emitted: `Skipping type [CompactCraftingTooltips.Research.Plugin] because its version is invalid.`
- The research plugin therefore never reached `Awake()`; none of the taxonomy diagnostics ran.
- Root cause: the `[BepInPlugin]` version string used the prerelease suffix `0.0.0-research`. BepInEx 5 parses that metadata through `System.Version` semantics, so the suffix is invalid for plugin metadata.

This artifact remains immutable and must not be reused.

### Research handoff r2 — accepted runtime evidence

Status: **accepted**

Correction:
- BepInEx plugin metadata version changed to numeric `0.0.0`.
- Project version changed to numeric `0.0.0`.
- Human-facing handoff identity is tracked externally as **r2**, not encoded as a prerelease suffix in `[BepInPlugin]`.
- No taxonomy logic or production behavior changed.

Artifact:
- identity: `CompactCraftingTooltips-Research-r2.dll`
- plugin/binary version: `0.0.0`
- handoff identity: `r2`
- source branch: `research/crafting-location-taxonomy`
- exact source SHA: `94088d20dcb507e0f7319db384321336346e4a4e`
- GitHub Actions run: `36571564735`
- GitHub Actions artifact ID: `11033843702`
- DLL SHA-256: `9b58d535af23ae1fc11cb0f39a06c44cdbc4fe6517ce418b0756f4f3c6345217`
- CI result: **success**
- handoff date: 2026-09-29

Research behavior:
- patches `MainGame.OnGameStartedPlaying()` only to trigger a one-shot read;
- enumerates native `GameBalance.items_data` and `GameBalance.GetItemCraftsIn(item_id)`;
- records ordered station IDs/names and selected raw `ObjectDefinition` metadata;
- reports numeric-suffix station groups only as diagnostic candidates;
- no save mutation;
- no UI mutation;
- no recipe/object mutation;
- no per-frame work.

Requested runtime test:
1. remove/replace the r1 research DLL so only `CompactCraftingTooltips-Research-r2.dll` is installed for this probe;
2. launch Graveyard Keeper 1.407 and load any save to normal gameplay;
3. after the game has reached normal gameplay, exit normally;
4. return the complete `BepInEx/LogOutput.log` from that session.

Expected diagnostic markers:
- `Compact Crafting Tooltips Research 0.0.0 loaded.`
- `CCT_RESEARCH_START`
- zero or more `CCT_ITEM`
- `CCT_STATION`
- zero or more `CCT_SUFFIX_CANDIDATE`
- `CCT_RESEARCH_DONE`

Returned runtime evidence, 2026-09-29:
- plugin loaded successfully under BepInEx 5.4.23.5;
- `CCT_RESEARCH_START game_version="1.407" language="ru" items=1157`;
- probe completed with `items_with_locations=702`, `items_with_multiple_locations=511`, `unique_stations=79`, `raw_links=1402`, `suffix_candidate_families=11`;
- no `CCT_RESEARCH_ERROR`;
- no save or UI mutation;
- data was sufficient to reject numeric-suffix-only grouping and to select a fail-closed native-ID + localized-name compatibility rule for production.

Probe result: **accepted; research question closed for the initial production formatter.**

This research artifact is not a production mod or release candidate.


## Production candidate 0.1.0

Status: **awaiting runtime visual acceptance**

Evidence gate: **READY** in `docs/VERIFIED_GAME_DATA.md`.

Candidate behavior:
- patches only `ItemDefinition.GetTooltipData(Item,bool)`;
- returns immediately unless `full_detail == true`;
- reads the same ordered native `GameBalance.GetItemCraftsIn(item_id)` locations as vanilla;
- groups only contiguous entries that share a terminal-numeric canonical ID family **and** an active-localization base/tier pattern;
- preserves partial families such as `II, III`;
- leaves renamed, reversed, mismatched, or unsupported runs unchanged;
- does not patch `GetTooltipDataCraftAt` or the `TechUnlock` `full_detail=false` path;
- no save, recipe, station, item-data, trading, research, or progression mutation;
- no per-frame work;
- production logging is startup-only unless the compactor throws, in which case the first failure is logged once and vanilla text is left unchanged.

Layout policy for this candidate:
- one compact family remains on the normal `Crafted at` line;
- when every displayed entry is a grouped family and there are at least two families, the localized `Crafted at` heading is placed on its own line and each compact family is placed on its own following line;
- mixed grouped + unrelated station lists stay inline and use the active localized list separator;
- ASCII comma/semicolon separators receive a trailing space for readability; non-ASCII localized separators are preserved as provided by the game.

Mechanical verification:
- GitHub Actions run: `36574067893`
- job: `test-and-build` — **success**
- formatter contract step: **success**
- production build step: **success**
- source branch: `dev/0.1.0`
- exact source SHA: `e16767986e2933d36a325c096101947a871ccfbe`
- artifact ID: `11036638165`
- installable assembly: `CompactCraftingTooltips.dll`
- DLL SHA-256: `5e8c9ab20837c2d3c21b6428745b756b219089a616f90a9980091e8d4569d477`

Minimum runtime acceptance:
1. remove the research-only `CompactCraftingTooltips-Research-r2.dll`;
2. install this exact `CompactCraftingTooltips.dll`;
3. inspect a beet/carrot/wheat tooltip that exposes Zombie Farm I–III + Garden Bed I–III;
4. inspect one partial-quality crop tooltip that exposes only II–III;
5. inspect one unrelated/mixed crafting-location tooltip if convenient;
6. return screenshots of the representative tooltip(s) and the complete `BepInEx/LogOutput.log`.

Required human judgment: readability, line breaks, wrapping, clipping, punctuation, and whether the compact block visually fits the rest of the vanilla tooltip.

Do not promote this candidate to stable `main` until the exact DLL above is accepted in the real game.
