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
