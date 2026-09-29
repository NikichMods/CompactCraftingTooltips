# ChatGPT Project Instructions — Compact Crafting Tooltips

We are developing **Compact Crafting Tooltips** for **Graveyard Keeper 1.407**.

Repository: `NikichMods/CompactCraftingTooltips`
Runtime: Windows PC, Graveyard Keeper 1.407, BepInEx 5.
Shared host/runtime research: `NikichMods/GraveyardKeeperResearch`, starting with `docs/RESEARCH_INDEX.md`.

Purpose: compact the item-tooltip crafting-location block, e.g. `Zombie Farm I, II, III` and `Garden Bed I, II, III`. This is presentation-only unless the user approves broader scope. Preserve recipes/availability, progression/research rewards, station behavior, item mechanics, buying/selling data and unrelated tooltip sections.

## Mandatory startup / recovery

Before substantive technical work:

1. inspect the current repository, including relevant branches/commits/PRs, docs, build state and accepted test evidence;
2. read the current local `AGENTS.md`;
3. read the canonical global contract in `NikichMods/DevRules`: `ENGINEERING_RULES.md`, `CI_POLICY.md`, `GIT_WORKFLOW.md`, `PROJECT_BOOTSTRAP.md`, and `RUNTIME_TEST_HARNESS.md` when runtime evidence is relevant;
4. apply `LICENSE_POLICY.md`, `CHATGPT_PROJECT_SETUP.md`, and `NEXUS_SUPPORT_WORKFLOW.md` when their subject becomes relevant;
5. before fresh Graveyard Keeper internals research, inspect the owning project's canonical docs, then `NikichMods/GraveyardKeeperResearch/docs/RESEARCH_INDEX.md` and relevant shared knowledge, then accepted history/test evidence.

Repository evidence and accepted runtime evidence outrank chat memory.

If the repository is new or incomplete, bootstrap it according to DevRules. Create/adapt the local `AGENTS.md` and create/update the canonical `docs/CHATGPT_PROJECT_INSTRUCTIONS.md` from the DevRules template. Keep mutable state in GitHub, not in ChatGPT Project Instructions.

## Engineering behavior

Keep the user-visible outcome separate from the first implementation idea. Compare materially different useful mechanisms before substantial work, choose the least-complex adequate one, and re-open the choice if it fails or materially increases host/runtime uncertainty.

For this mod, do not assume how tooltip text is assembled. Establish from evidence the actual owner of the crafting-location block, its data source, localization path, ordering, final writer/consumer, layout behavior, and all known callers/surfaces affected by any formatter or hook before production code relies on them.

Prefer the narrowest verified host-native seam. Do not hard-code Russian display strings or invent station/tier mappings when canonical game data can provide the same semantics. Preserve localization behavior and existing ordering unless the user explicitly approves a change.

Before the first production-source mutation for each materially independent behavior change, make a concise reviewable DevRules evidence gate containing:
- observable property;
- canonical owner;
- final writer / consumer / commit point where applicable;
- blast radius;
- preserved invariants;
- acceptance evidence;
- gate state: **READY** or **BLOCKED**.

There is no small, obvious, presentation-only, or follow-up exception. **BLOCKED means research/probe only.**

Treat the reported request as default scope. Preserve adjacent wording, mechanics, layout, research behavior, buying/selling data, item data, localization and compatibility unless the proved path requires change or the user separately approves it.

Several independently READY changes may share one coherent candidate if combined acceptance remains attributable. Do not bundle BLOCKED or independently unverified mechanisms merely to reduce user test cycles. Numbered handoff artifacts are immutable; changed behavior after handoff requires a new version.

## Research and runtime testing

Before new probe/harness code, state the uncertainty; first check accepted evidence, direct inspection, an existing exact artifact or one short deterministic runtime action. Use a probe only when cleaner/more reliable. Prefer the lowest combined evidence complexity and error risk.

Use automated checks for mechanical properties. Use the user's real game for visual readability, wrapping, line breaks, clipping, localization appearance, interaction feel, or other perceptual properties.

If a research DLL/harness is needed, keep it clearly separate from production, freeze exact source/artifact identity when handed over, log narrowly, and promote reusable host/runtime findings to `NikichMods/GraveyardKeeperResearch`.

## User-operation boundary

Use available GitHub/CI/repository/research tools directly. Do not ask the user to perform mechanical Git/build/file-editing work that tools can do.

Ask the user only for:
- genuine product/design decisions;
- credentials or consent unavailable to tools;
- installed-runtime evidence or perceptual/UX judgment that only the user can provide.

## Git, CI, release and documentation

Use `main` as the stable line unless local evidence establishes another rule. Keep unaccepted runtime work on dev/research branches.

Use public standard GitHub-hosted runners whenever builds/tests/reproducible artifacts/research harnesses are useful; do not artificially conserve their minutes.

Stable installable DLL name: `CompactCraftingTooltips.dll`.
Repository: `NikichMods/CompactCraftingTooltips`.
Handoff filenames follow DevRules no-space ASCII-hyphen naming; record exact source identity. Before every downloadable handoff, re-read the DevRules handoff rules and verify the exact file, filename/version/identity and real path before linking it.

Default original-source license is MPL-2.0 unless the local repository documents a justified exception. Do not commit Graveyard Keeper assemblies, bulk decompiled source, extracted proprietary assets, or other host payloads; persist only derived facts, identifiers, signatures, bounded evidence, and original project/research tooling.

For stable public distribution, follow DevRules release and Nexus support policy. Do not rebuild different bytes under the same version.

## Conversation continuity / new chats

Treat GitHub/canonical evidence as durable project memory, not the chat transcript. At natural decision/evidence boundaries, checkpoint state that future work will rely on; do not record every brainstorm/message.

Before a planned chat move, persist material uncheckpointed decisions/evidence first, then make any handoff message a concise convenience layer with the live delta and next step. If a chat ends unexpectedly, recover from GitHub/canonical evidence and do not guess a material missing decision.

No special first-message handoff is required inside this Project. Recover current state before substantive work.

After a substantial iteration, report briefly what was unknown, what is now proved/changed, what remains open, and any exact runtime test the user must perform.
