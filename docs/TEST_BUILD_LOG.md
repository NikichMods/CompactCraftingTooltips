# Test / Research Build Log

## Crafting-location taxonomy probe

Status: **awaiting runtime evidence**

Purpose: close the remaining production evidence-gate question for Compact Crafting Tooltips: identify the complete live Graveyard Keeper 1.407 crafting-station taxonomy used by `GameBalance.GetItemCraftsIn` and determine whether tiered station families can be grouped generically without hard-coded display strings or an unsafe numeric-suffix assumption.

Research artifact:
- identity: `CompactCraftingTooltips-Research.dll`
- research version: `0.0.0-research`
- source branch: `research/crafting-location-taxonomy`
- exact source SHA: `a56bec010603b5224ad42cb36a244a90abfc00a9`
- GitHub Actions run: `36471545218`
- GitHub Actions artifact ID: `10992280727`
- DLL SHA-256: `372d3dc600f02f87493dd9b6f0419962d6e624658747c7cc7bcf19d2f71d763e`
- CI result: **success**
- handoff date: 2026-09-28

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
1. place only `CompactCraftingTooltips-Research.dll` from this handoff in `BepInEx/plugins`;
2. launch Graveyard Keeper 1.407 and load any save to normal gameplay;
3. after the game has reached normal gameplay, exit normally;
4. return the complete `BepInEx/LogOutput.log` from that session.

Expected diagnostic markers:
- `CCT_RESEARCH_START`
- zero or more `CCT_ITEM`
- `CCT_STATION`
- zero or more `CCT_SUFFIX_CANDIDATE`
- `CCT_RESEARCH_DONE`

Acceptance condition for the probe itself: it reaches `CCT_RESEARCH_DONE` without `CCT_RESEARCH_ERROR` and the returned data is sufficient to classify genuine station families or establish that another evidence step is required.

This research artifact is not a production mod or release candidate.
