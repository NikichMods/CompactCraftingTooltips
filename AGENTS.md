# Project Working Contract

This repository follows the canonical global development rules in `NikichMods/DevRules`.

Before substantive technical work, read:
- `ENGINEERING_RULES.md`
- `CI_POLICY.md`
- `GIT_WORKFLOW.md`
- `PROJECT_BOOTSTRAP.md`
- `RUNTIME_TEST_HARNESS.md` when installed-runtime evidence is relevant
- `LICENSE_POLICY.md`, `CHATGPT_PROJECT_SETUP.md`, and `NEXUS_SUPPORT_WORKFLOW.md` when their subject is relevant

This file contains only project-specific additions, constraints, verified facts, and explicit exceptions.

## Project identity

- Project name: **Compact Crafting Tooltips**
- Repository: `NikichMods/CompactCraftingTooltips`
- Target/runtime: **Graveyard Keeper 1.407**, Windows PC, BepInEx 5
- Stable installable DLL: `CompactCraftingTooltips.dll`
- Purpose: make the item-tooltip crafting-location block compact and readable by grouping repeated tiered variants of the same crafting station.

## Scope

This is presentation-only unless the user explicitly approves a broader change.

Initial product outcome: replace long repeated station lists with compact grouped entries, for example conceptually:
- `Zombie Farm I, II, III`
- `Garden Bed I, II, III`

Preserve by default:
- recipes and recipe availability;
- progression and research rewards;
- station behavior;
- item mechanics/data;
- buying/selling data;
- unrelated tooltip sections;
- existing localization semantics and station ordering;
- compatibility behavior outside the proved seam.

Do not hard-code Russian display strings or create manual station/tier mappings when canonical game data can express the same semantics. Do not infer family/tier identity merely by stripping display-text suffixes unless evidence proves that is the least-sufficient reliable mechanism.

## Mandatory project-specific start-of-work checks

Before substantive implementation:
1. inspect current repository state and this file;
2. inspect `docs/VERIFIED_GAME_DATA.md` and other relevant project docs/history/test evidence;
3. inspect the shared Graveyard Keeper research source below;
4. verify the actual owner, data source, localization path, ordering, final writer/consumer and blast radius of the crafting-location block before production code relies on them;
5. keep research/probes separate from production.

Repository evidence and accepted runtime evidence outrank chat memory.

## Shared Graveyard Keeper research

Cross-project Graveyard Keeper 1.407 research is centralized in `NikichMods/GraveyardKeeperResearch`.

Before fresh host-internals research:
1. read this project's canonical docs;
2. read `NikichMods/GraveyardKeeperResearch/docs/RESEARCH_INDEX.md` and linked relevant knowledge;
3. inspect accepted local/shared logs and history;
4. only then perform fresh static inspection or a narrow runtime probe.

Promote accepted reusable host/runtime facts back to the shared research repository. Keep project-specific UX, implementation, release state, and acceptance evidence here.

## Project-specific evidence contract

The standard item-tooltip seam `ItemDefinition.GetTooltipData(Item, bool)` is established by shared research, but that fact alone does not prove ownership of the vanilla crafting-location block.

Before the first production mutation for the compacting behavior, the DevRules evidence gate must establish:
- the observable property;
- canonical owner of the crafting-location entries;
- the final writer/consumer/commit point;
- all known tooltip surfaces/callers affected by the chosen seam;
- preserved invariants;
- acceptance evidence;
- explicit gate state **READY** or **BLOCKED**.

**BLOCKED means research/probe only.** There is no presentation-only exception.

## Architecture / runtime constraints

Prefer the narrowest verified host-native seam and host-owned data.

Avoid:
- per-frame work or polling when tooltip creation/data generation is sufficient;
- broad UI replacement;
- mutating recipe/station definitions for presentation;
- parsing localized text when stable semantic data is available;
- duplicate localization tables for vanilla station names;
- caches or mappings without a demonstrated need.

When grouping, preserve vanilla ordering unless the user approves a change. Localization changes must remain correct without requiring a manual per-language station table when the host provides the names.

## User-facing behavior requirements

The mod should reduce visual noise in the crafting-location section without changing what locations are represented.

A compact family should remain understandable in the active game language. Visual readability, wrapping, clipping, punctuation, line breaks and overall tooltip balance require real-game acceptance when they cannot be proved mechanically.

## Git / version / acceptance workflow

- `main` is the stable/documentation baseline.
- Research-only work uses `research/<topic>`.
- Build-bearing production work uses `dev/<version>` or a semantic feature branch.
- Research-only work does not consume numbered release/test versions.
- Runtime behavior reaches `main` only after exact candidate/runtime evidence and explicit user acceptance.
- Numbered handed artifacts are immutable.
- Stable public distribution uses GitHub Releases unless the local contract is deliberately changed.
- Handoff/download filenames use ASCII hyphens and no spaces; installed DLL remains `CompactCraftingTooltips.dll`.

## Licensing

Original project software source uses MPL-2.0 under the global `LICENSE_POLICY.md`. See `LICENSE` and `LICENSING.md`.

Do not commit Graveyard Keeper assemblies, bulk decompiled source, extracted proprietary assets, or other host payloads. Preserve only derived facts, identifiers, signatures, bounded evidence, and original project/research tooling.

## CI / build specifics

This is a public repository. Standard GitHub-hosted runner minutes are not scarce. Use hosted CI whenever compilation, tests, reproducible artifacts, or research harness builds are useful.

Do not establish a project-specific compiler/package contract by assumption; derive the actual build setup from verified compatible NikichMods/BepInEx practice and this project's source requirements when production/research code is introduced.

## Long-lived sources of truth

- `AGENTS.md`
- `docs/VERIFIED_GAME_DATA.md`
- `docs/CHATGPT_PROJECT_INSTRUCTIONS.md`
- `docs/TEST_BUILD_LOG.md` once numbered handoff builds exist
- `README.md` / `CHANGELOG.md` when applicable

Mutable candidate state belongs in repository docs/history, not ChatGPT Project Instructions.
