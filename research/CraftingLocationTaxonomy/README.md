# Crafting Location Taxonomy Research Probe

Research-only BepInEx plugin for **Compact Crafting Tooltips**.

## Question

The vanilla owner, source and order of the `Crafted at` row are already established. The remaining blocker is whether live Graveyard Keeper 1.407 balance data exposes a reliable generic station-family/tier relationship, rather than requiring hard-coded display strings or an unsafe numeric-suffix assumption.

## Method

After `MainGame.OnGameStartedPlaying()`, the probe runs once and reads:

- every item from `GameBalance.items_data`;
- its native ordered `GameBalance.GetItemCraftsIn(item_id)` list;
- each unique station's ID, active localized name, craft preset, craft subtype, interaction type, sort order, `has_craft`, and raw object-group IDs;
- numeric-suffix groups as **candidates only**, alongside the raw data.

The candidate lines are diagnostic conveniences, not proof that a numeric suffix means a tier.

## Safety

- research-only;
- read-only;
- no save mutation;
- no UI mutation;
- no recipe/object mutation;
- no per-frame work;
- one dump per process after gameplay starts.

## Runtime handoff

Install only the built research DLL in `BepInEx/plugins`, load any save to normal gameplay once, then return the complete `BepInEx/LogOutput.log`.

The relevant block is delimited by `CCT_RESEARCH_START` and `CCT_RESEARCH_DONE`.


## Handoff compatibility note

BepInEx 5 requires the version in `[BepInPlugin]` to be numeric/System.Version-parseable. The first research handoff used `0.0.0-research` and was rejected before `Awake()` ran. Research handoff **r2** uses numeric plugin/binary version `0.0.0`; the human-facing research identity is tracked outside BepInEx metadata.
