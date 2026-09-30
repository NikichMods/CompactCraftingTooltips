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


## Production candidate 0.1.1

Status: **awaiting runtime visual acceptance**

Scope added after accepted 0.1.0 visual review:
- Sweet Home `cooking_table -> cooking_table_2` compacts to `Cooking table I, II`;
- refugee-camp `refugee_camp_cooking_table -> _2` compacts to a location-qualified family, e.g. `Refugee camp: Cooking table I, II`;
- the refugee-camp qualifier is mod-owned only because no reusable native full `Refugee camp` GJL key was established; station names themselves remain native localization;
- `mf_distcube_2_clay -> mf_distcube_2_cuprum` compacts to `Distillation cube I, II`;
- modern `mf_alchemy_craft_02 -> mf_alchemy_craft_03` compacts to `Alchemy workbench I, II` using the active first-member base text;
- exact duplicate player-facing zombie-mine pair `zombie_mine_fence_front -> zombie_mine_fence_left_front` collapses to one `Zombie Mine` entry and is explicitly **not** relabeled as tiers;
- `Professional Kitchen`, `Professional Oven`, `Wooden Anvil`, Stone deposit and Marble quarry remain distinct.

Localization qualifier coverage:
- English, French, German, Simplified Chinese, Spanish, Brazilian Portuguese, Korean, Japanese, Russian, Italian and Polish;
- unknown/unsupported locale falls back to English qualifier only;
- formatter contract mechanically verifies the embedded qualifier resources.

Mechanical verification:
- GitHub Actions run: `36586934398`
- workflow run number: `17`
- job: `test-and-build` — **success**
- formatter contract: **success**
- production restore/build: **success**
- source branch: `dev/0.1.1`
- exact source SHA: `286a7a36123852b38293107188f91fe21ade7345`
- artifact ID: `11042695597`
- installable assembly: `CompactCraftingTooltips.dll`
- DLL SHA-256: `9621f5f838077a5d23026f8e0a02046c88928c2de5e4004a5a4d6bc9d2f8c6b2`
- CI artifact zip SHA-256: `9d9d7fd4da11fbd5bc0fceedff8a1e533f29bbab28dfd2a6c7e3d897319e88f1`

Intermediate CI failures before this source identity were build/test-harness resource-embedding issues only; they did not produce a handed production candidate. Run 17 is the first accepted mechanical build for 0.1.1.

Requested runtime acceptance:
1. replace the previous `CompactCraftingTooltips.dll` with this exact 0.1.1 candidate;
2. inspect at least one item containing the home Cooking Table pair;
3. inspect one item containing both home and refugee-camp Cooking Table families if available (for example dough);
4. inspect one Distillation Cube pair;
5. inspect one modern Alchemy Workbench I/II pair;
6. inspect stone for the duplicate Zombie Mine collapse;
7. return representative screenshots and the complete `BepInEx/LogOutput.log`.

Required human judgment: wording, readability, wrapping/clipping, punctuation, refugee-camp distinction, and whether all unchanged stations still read naturally.

Do not promote 0.1.1 to stable `main` until this exact DLL is accepted in the real game.


## Runtime acceptance — 0.1.1

Status: **ACCEPTED**

Accepted by the user after real-game testing on Graveyard Keeper 1.407 (Steam, Windows) with the user's normal mod set.

Evidence:
- BepInEx loaded `Compact Crafting Tooltips 0.1.1` successfully.
- The session's mod summary reported `Compact Crafting Tooltips v0.1.1`.
- No `Crafting-location compaction failed` or `failed to initialize` entry occurred in the returned complete log.
- The returned screenshot for sweet dough showed the intended simultaneous distinction:
  - `Кухонный стол I, II`
  - `Лагерь беженцев: Кухонный стол I, II`
  - `Профессиональная кухня`
  with readable wrapping and no clipping.
- The user reported no problems across the other requested spot checks and explicitly accepted the candidate for stable promotion.

Accepted exact candidate identity:
- version: `0.1.1`
- source SHA: `286a7a36123852b38293107188f91fe21ade7345`
- CI run: `36586934398`
- artifact ID: `11042695597`
- DLL SHA-256: `9621f5f838077a5d23026f8e0a02046c88928c2de5e4004a5a4d6bc9d2f8c6b2`

Note: an exact host-build compatibility guard was discussed separately after this candidate was built. It is **not** present in 0.1.1 and is not part of the accepted runtime behavior above. Adding such a guard would be a separate behavior change requiring an explicit decision and a new candidate identity before release.


## Conversation checkpoint — next compatibility/logging hardening

Status: **design direction accepted; no production mutation yet**

The user explicitly accepted production candidate 0.1.1 in real Graveyard Keeper 1.407, but then deferred stable promotion/release until compatibility and support-diagnostics hardening is complete. Do **not** merge/release 1.0.0 yet; the user will make that decision separately after the next candidate is accepted.

Accepted direction for the next production candidate:

- use capability-first, identity-aware compatibility;
- known Graveyard Keeper 1.407 Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364` is reported as verified;
- a different/unknown host identity lowers confidence but does not by itself block this presentation-only feature;
- resolve/check only the exact runtime contract CCT needs at startup and cache it; no repeated MVID/reflection scans in tooltip/hot paths;
- if a required contract is absent, fail closed at the CCT boundary and preserve vanilla behavior; do not guess alternate hooks or fuzzy-discover replacement APIs;
- if a contract-breaking runtime exception occurs after startup, preserve the vanilla tooltip and trip a one-session circuit breaker so CCT stops retrying that feature until restart;
- log the failure/containment once rather than on every tooltip;
- keep compatibility code local and small; do not introduce a shared runtime framework merely for this mod;
- accepted 0.1.1 formatting/localization/order behavior must remain unchanged on the verified 1.407 host.

Support-log direction:
- normal startup should be sparse and identify component/version plus host compatibility status;
- unknown host should produce one concise unverified/best-effort warning;
- a disabled/contained feature should report what failed and what fallback/action occurred (for example `fallback=vanilla`, `action=disabled-for-session`);
- no per-tooltip success logging or recurring failure spam.

Verification direction:
- prove project-owned compatibility/failure-state logic with deterministic automated tests first;
- use narrow controlled fault injection only if a real integration boundary cannot be proved by ordinary tests;
- use the user's real 1.407 game only for the remaining runtime assertion, expected to be a short startup/tooltip sanity check plus the ordinary `LogOutput.log` if lower layers already prove the negative paths;
- do not attempt to simulate every possible future Graveyard Keeper version.

Known deferred issue from 0.1.1:
- `CraftingTooltipCompactor.TryCompact` currently returns before formatting when the native location count is below two, while the formatter itself can qualify a singleton refugee-camp cooking-table entry. Current accepted collision cases contain multiple locations. Any singleton-behavior change is a separate future behavior gate; do not silently fold it into compatibility hardening.

Next step after chat recovery:
1. perform the normal DevRules/repository/shared-research startup inspection;
2. re-verify the exact 0.1.1 source/runtime baseline and current compatibility seam;
3. create the reviewable evidence gate for compatibility/logging hardening before the first production-source mutation;
4. implement as a new candidate identity (0.1.1 is immutable; `0.1.2` is the expected next candidate unless current repository evidence establishes a better version);
5. build/test in CI and hand over only after the DevRules artifact-handoff integrity check.


## Compatibility / support-logging hardening evidence gate — 0.1.2

Status: **READY**

Baseline:
- immutable accepted production behavior: 0.1.1 source `286a7a36123852b38293107188f91fe21ade7345`;
- current development checkpoint before this gate: `52fa6ff715c4fba1b81bd4b8960db5d8e87b4802`;
- stable promotion / 1.0.0 / release remain explicitly deferred;
- the known singleton refugee-camp qualifier edge case remains outside this change.

Observable property:
- at plugin startup, resolve and validate only the exact host contract required by Compact Crafting Tooltips, cache it, and decide activation before installing the Harmony patch;
- an unknown host identity alone remains non-blocking when the exact contract is present;
- a missing required contract disables CCT and preserves vanilla behavior;
- a contract-breaking exception after activation preserves the vanilla tooltip for that call, disables CCT for the rest of the process session, and logs containment once;
- ordinary successful operation performs no per-tooltip compatibility discovery or success logging.

Canonical owner / verified seam:
- tooltip producer and patched method: `ItemDefinition.GetTooltipData(Item,bool)`;
- native crafting-location source/order: `GameBalance.GetItemCraftsIn(string)`;
- station identity: `BalanceBaseObject.id`;
- localization: exact `GJL.L(string)`;
- current language: `GameSettings.GetCurrentLanguage()`;
- mutable tooltip text row: `BubbleWidgetTextData.text`;
- final CCT write remains the existing single `SetBubbleText` call after exact vanilla-row matching.

Additional runtime contract evidence:
- verified Graveyard Keeper 1.407 `Assembly-CSharp` MVID remains `6f50b8e7-156b-49ac-bbe8-7505894b2364`;
- direct metadata inspection of the user's installed 1.407 `Assembly-CSharp-firstpass.dll`:
  - SHA-256: `9dc6def3b7715dd27eeb168ddc0af47e31c6f38d3fbee24bf592899392026498`;
  - module MVID: `7b81560f-fee5-4bdd-ac8c-058e486c8a3f`;
  - `GJL.L` method RID 219 is public static and has exact signature `string L(string lng_id)`;
  - the same type also contains multiple 2–4 argument `L` overloads, so production must bind the exact one-argument signature and must not retain the previous compatible-overload fallback scan.
- pinned 1.407 `GameBalance.me` is a public static field, so the previous field/property fallback is not part of the required production contract.

Blast radius:
- startup host identity telemetry;
- startup reflection binding / exact-contract validation;
- Harmony installation only after the contract is complete;
- one-session failure containment around the existing tooltip compactor.
- No formatter, localization-resource, grouping, station-order, recipe, item, progression, trading, save-data, or unrelated tooltip behavior is in scope.

Preserved invariants:
- accepted 0.1.1 formatting, localized station names, qualifier resources, native ordering, vanilla-row matching, and layout remain unchanged on verified 1.407;
- `locations == null || locations.Count < 2` remains unchanged;
- unknown MVID is a warning / lower-confidence identity only, not an activation veto;
- no speculative alternate type/member discovery and no MVID/reflection work in the tooltip hot path.

Acceptance evidence:
1. deterministic automated tests for host-identity classification and the one-session circuit-breaker/log-once state;
2. production compile plus existing formatter contract tests;
3. static review that all host binding is exact and cached before patch installation;
4. only if an integration-boundary assertion remains unproved, add narrow fault injection; otherwise do not add a runtime harness merely to test defensive policy;
5. final real-game evidence is limited to a short 1.407 startup + representative tooltip sanity check and ordinary `LogOutput.log`.

Gate state: **READY**.


## Production candidate 0.1.2

Status: **awaiting corrected runtime sanity candidate**

Pre-handoff correction:
- CI run `36649562784` / source `482a1869044a96228e968c9df28ad99e1306b4f0` built successfully but was **rejected before handoff** by final static review;
- reason: that source incorrectly required `GameBalance.me` as a property, while pinned 1.407 source defines it as `public static GameBalance me;`;
- those bytes were never handed to the user and are not the 0.1.2 candidate of record;
- corrected source begins at `cdcafde9148fcc0693d2c8a422543deab4312670`; final CI/artifact identity follows after its build completes.

Scope:
- compatibility and support-logging hardening only;
- accepted 0.1.1 formatter, localization resources, native station ordering, row matching, and layout logic are unchanged;
- exact startup contract binding replaces broad/fallback reflection discovery;
- verified Graveyard Keeper 1.407 `Assembly-CSharp` MVID is recognized as trusted identity telemetry;
- an unknown MVID is logged once as unverified/best-effort but does not block activation when the exact contract is present;
- a missing required contract fails closed before Harmony installation, leaving vanilla behavior;
- after activation, the first unexpected compaction exception trips a one-session circuit breaker, logs containment once, and prevents further compaction attempts until restart;
- no MVID lookup or reflection scan/check is performed in the tooltip hot path;
- singleton refugee-camp behavior remains unchanged and out of scope.

Exact startup contract:
- `Assembly-CSharp:ItemDefinition.GetTooltipData(Item,bool) -> IList-compatible`;
- `Assembly-CSharp:GameBalance.me` public static `GameBalance` field;
- `Assembly-CSharp:GameBalance.GetItemCraftsIn(string) -> IList-compatible`;
- `Assembly-CSharp:BalanceBaseObject.id` public instance `string` field;
- `Assembly-CSharp:BubbleWidgetTextData.text` public instance `string` field;
- `Assembly-CSharp:GameSettings.GetCurrentLanguage() -> string`;
- `Assembly-CSharp-firstpass:GJL.L(string) -> string`.

Mechanical verification:
- source branch: `dev/0.1.2`;
- exact candidate source SHA: `482a1869044a96228e968c9df28ad99e1306b4f0`;
- GitHub Actions run: `36649562784`;
- workflow run number: `19`;
- job: `test-and-build` — **success**;
- formatter + compatibility contract harness: `FORMATTER_CONTRACT_OK checks=72`;
- production build: **success**;
- artifact ID: `11069629520`;
- CI artifact digest: `sha256:a0da69f6c9a1e2733f13438173197a46abd6d9043286a8e7c0c4f71464228898`;
- installable assembly: `CompactCraftingTooltips.dll`;
- candidate DLL SHA-256: `45ef87fefa7560efdca66ab1bfb62b746964506cbc0f77e39c57f33224c1c26f`;
- handoff filename: `CompactCraftingTooltips-0.1.2.dll`.

Defensive verification decision:
- no runtime fault-injection harness is added for this candidate;
- project-owned identity classification and circuit-breaker/log-once state are covered deterministically in CI;
- fail-closed startup ordering is structurally enforced because the Harmony patch is installed only after the complete exact contract bind succeeds;
- the remaining real integration assertion is therefore the ordinary verified-1.407 startup/bind/patch path plus one representative tooltip.

Requested runtime sanity:
1. replace the previous production DLL with this exact 0.1.2 candidate;
2. launch Graveyard Keeper 1.407 and load a normal save;
3. inspect one previously accepted tooltip that CCT compacts and confirm its visible output is unchanged from 0.1.1;
4. exit normally and return the complete `BepInEx/LogOutput.log`.

Expected verified-host startup marker:
`Compact Crafting Tooltips 0.1.2 active; host=verified-gk-1.407; assembly-csharp-mvid=6f50b8e7-156b-49ac-bbe8-7505894b2364; contract=ok.`

Do not promote 0.1.2 to `main`, 1.0.0, or a public release until the user separately accepts this exact candidate.
