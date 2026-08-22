# Niko — Engineering Rules

This file is the source of truth for all agents, developers, and AI tools working on Niko, a nicotine-cessation app. These rules override assumptions, default conventions, and tool suggestions.

## 1. Non-negotiable principles

- **MUST** keep implementation aligned with the documents in `docs/`.
- **MUST** preserve offline-first, privacy-first behavior across mobile, wearables, and widgets.
- **MUST** keep domain logic in the shared Core. UI, widgets, and companions are adapters/presentation only.
- **MUST** define the event, local persistence, sync, and failure path for every new capability.
- **MUST NOT** send sensitive health or consumption data to external services without explicit user consent.
- **MUST NOT** hard-code UI text, error messages, button labels, or coach content.
- **SHOULD** choose the smallest testable solution and avoid unnecessary dependencies.

## 2. Persian code-comment requirement

Every new code file/page **MUST** start with a Persian documentation comment containing:

1. file name;
2. responsibility;
3. dependencies and layer relationships;
4. important change notes and constraints.

Complex or non-obvious logic must have concise Persian comments. Do not over-comment, repeat method names, or explain obvious code. This applies to C#, XAML, TypeScript/JavaScript, Swift, Kotlin, contract/configuration files, and similar code artifacts.

## 3. Architecture boundaries

- **Shared Core:** domain models, use cases, events, policies, localization contracts, sync contracts, and storage/network abstractions.
- **Infrastructure:** local storage, SQLite, sync queue, transport, encryption, and platform adapters.
- **.NET MAUI:** primary mobile UI and composition; no independent domain rules.
- **Wear OS / watchOS:** native companions for QuickLog and short feedback; Core/Sync remains the source of truth.
- **Android/iOS widgets:** limited input and display; every action goes through a shared use case.
- **AI Coach / Trigger Analysis:** behind explicit interfaces, minimum-data access, and a user-controlled disable path.

Any architecture bypass **MUST** be justified in `docs/DECISIONS.md`.

## 4. Localization

- **MUST** use key/resource-based localization; no UI text may be hard-coded.
- **MUST** support at least 15 languages, fallback language, RTL/LTR, and language changes without domain-code changes.
- **MUST** format dates, times, numbers, units, percentages, and currencies using the active locale.
- **MUST NOT** assume fixed text length; layouts must tolerate expansion, fonts, and direction changes.

## 5. Change workflow

1. Read `AGENTS.md` and the relevant documents before changing code.
2. For architecture, domain, sync, privacy, or schema changes, write a short plan and impact note first.
3. Implement the smallest testable change.
4. Run relevant unit, integration, and UI tests.
5. Run builds for affected targets and fix new failures.
6. Record migrations, risks, limitations, and user-visible changes in the change description/commit.

## 6. Build and test rules

- **MUST** run restore, build, and relevant tests before delivery.
- **MUST** provide Core unit tests, persistence/sync integration tests, and offline QuickLog coverage where applicable.
- **SHOULD** keep tests deterministic and independent of real networks.
- Report environment failures separately from code regressions; never hide failures.

## 7. Dependency policy

- **MUST NOT** add a dependency without justification, license review, maintenance review, security/size assessment, and review approval.
- Prefer official .NET/MAUI libraries and small internal abstractions.
- Record platform and build impact for dependency version changes.

## 8. Data compatibility

- Schemas and event contracts must be versioned and backward-compatible.
- Destructive migrations, data deletion, or semantic event changes require a migration path and appropriate consent.
- Every log event must have a unique ID, timestamp, source, event type, and sync status.

## 9. Git and commits

- Keep commits small, atomic, and clearly described.
- **MUST NOT** commit secrets, tokens, real user data, or private dumps.
- Review the diff, new files, tests, and migration state before committing.
- Files under `sources/` are synced read-only references and must not be edited, moved, or deleted.

## 10. Agent roles

- **DeepSeek Flash:** routine implementation and bounded changes within these rules.
- **Codex and ChatGPT:** architecture review, debugging, code review, privacy/safety review, and quality control.
- No agent may bypass localization, privacy, Core boundaries, or tests to fix a bug quickly.

