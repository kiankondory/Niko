# Niko — Architecture and Product Decisions

Record durable decisions and their rationale here. Every new decision must include date, status, context, and consequences.

## ADR-012 — P2.2 local coach foundation

- Date: 2026-08-20
- Status: Accepted
- Context: AI Coach must remain privacy-first before any external provider exists.
- Decision: The first foundation is local and deterministic. It uses only approved
  aggregate context, is disabled by default, and has no provider call, API key,
  backend, raw note, or unrestricted event-history path.
- Consequence: Core owns the contracts and safety policy. SQLite schema version 7
  adds one additive `coach_preferences` row table; clearing coach data removes only
  that row and does not alter profile, event, or locale data.

## ADR-013 — P2.3 external provider readiness boundary

- Date: 2026-08-20
- Status: Accepted
- Context: Prepare a future external Coach adapter without selecting a vendor or
  creating a network path before provider, security, and consent review.
- Decision: Core defines a provider-neutral contract and privacy gateway. The app
  registers an unavailable no-network adapter, while external consent remains a
  separate persisted preference and defaults to false. The gateway forwards only
  approved aggregate context and always carries the local fallback.
- Consequence: No SDK, endpoint, API key, secret, provider selection, or network
  call is introduced. A real adapter must pass the gateway and safety policy later.

## ADR-014 — P2.4 Gemini Free-Tier backend proxy

- Date: 2026-08-20
- Status: Accepted
- Context: The first external provider must be reachable only through a secure
  backend and must not create billing or a credential path in the mobile app.
- Decision: Use a provider-neutral backend adapter for Gemini `generateContent`.
  The backend reads `GEMINI_API_KEY` from its secret environment, requires an
  authenticated HTTPS proxy request, accepts only the approved aggregate Coach
  context, allow-lists explicitly free-tier models, applies local output safety
  validation, and maps every failure to the local deterministic fallback.
- Alternatives considered: Direct MAUI Gemini calls (rejected: exposes the key),
  paid model fallback (rejected: violates free-tier scope), and a shared secret
  embedded in the APK (rejected: not a secure authentication boundary).
- Consequences and trade-offs: A real device deployment needs secure runtime
  authentication provisioning. Without it, external Coach stays unavailable and
  local Coach remains fully functional. Free-tier limits can change by model and
  project, so quota budgets are conservative and configurable.

## ADR-015 — P2.5 fail-closed free-provider availability

- Date: 2026-08-21
- Status: Accepted
- Context: External Coach must never become a paid feature or appear available
  when its free-only, health, quota, billing, or authentication state is unknown.
- Decision: Core exposes explicit availability states and requires `AvailableFree`,
  explicit billing-disabled evidence, no paid fallback, and provider health before
  forwarding any approved context. The backend reports this state through an
  authenticated health endpoint. Backend authentication uses short-lived HMAC
  session tokens; absent or invalid sessions fail closed.
- Alternatives considered: Treating missing billing/quota values as safe (rejected:
  uncertainty could enable paid usage), a long-lived shared token (rejected:
  unsafe session boundary), and enabling the UI based only on local configuration
  (rejected: the mobile app cannot verify provider availability).
- Consequences and trade-offs: Without a securely provisioned session and verified
  free backend state, External Coach remains disabled while Local Coach works offline.

## ADR-016 — P3.1 persistent companion idempotency
- Date: 2026-08-21
- Status: Accepted
- Context: Native companions can redeliver messages after process death or launcher retries.
- Decision: Store only Companion `MessageId` and processing time in an additive SQLite table,
  and keep duplicate rejection behind the existing `IProcessedMessageStore` contract.
- Alternatives considered: Session-only memory (rejected: loses idempotency after restart)
  and storing full messages (rejected: unnecessary privacy and storage exposure).
- Consequences and trade-offs: The table grows with delivered message identifiers; event data
  remains unchanged and existing in-memory test doubles remain valid.

## ADR-017 — P3.2 Wear OS Companion transport boundary
- Date: 2026-08-21
- Status: Accepted
- Context: P3.2 needs a small Wear OS QuickLog companion without creating a
  second domain store or requiring a watch during development.
- Decision: Add a native Wear OS adapter that emits the existing Companion
  envelope with `EventSource.Wearable` (2), stable `MessageId`/`EventId`,
  contract version, event type, and UTC timestamps. Failed delivery is kept in
  a minimal durable Wear queue and retried with the same serialized message.
  Summary data is accepted only as validated aggregate fields from the Phone.
  The default transport is unavailable and has no network or backend path.
- Alternatives considered: Reimplementing QuickLog on the watch (rejected:
  violates Core ownership), storing events on the watch (rejected: second
  source of truth), and adding a direct backend transport (rejected: outside
  scope and privacy boundary).
- Consequences and trade-offs: The native project requires the Android/Wear
  Gradle toolchain for compilation. Until a Phone Data Layer transport is
  connected, actions remain pending locally and the watch shows a safe
  unavailable state; no user data is lost on the Phone.

### ADR-018 — P3.2 Phone/Wear Data Layer delivery
- Date: 2026-08-22
- Status: Accepted
- Context: P3.2 needs a phone-owned delivery path for the existing Wear
  Companion envelope without adding backend connectivity or a second store.
- Decision: Use the official Google Play Services Wearable Data Layer. Wear
  sends the existing versioned QuickLog JSON on `/niko/companion/quicklog`;
  the Phone `WearableListenerService` forwards it unchanged to
  `CompanionUseCase`, where source validation, event validation, and durable
  MessageId/EventId idempotency remain centralized. Transport failures stay in
  the existing Wear pending queue.
- Consequences and trade-offs: The Phone and Wear builds require the official
  Wearable binding/dependency. Aggregate summary responses remain on the safe
  unavailable fallback until the bidirectional response path is added and
  tested; no raw event history or private metadata is sent to Wear.

## ADR-020 — P3.3 Trigger Analysis UI feature flag

- Date: 2026-08-22
- Status: Accepted
- Context: Phase 3 requires staged release and rollback safety for optional UI capabilities.
- Decision: Trigger Analysis UI is controlled by a platform-neutral Core flag contract and a MAUI runtime environment adapter. The default is enabled for existing users; explicit false values and unknown values disable only the UI. Core calculations and persisted preferences remain unchanged.
- Consequences and trade-offs: A deployment can hide the Trigger Analysis section without deleting data or changing domain behavior. Re-enabling the flag restores the existing UI path.

## ADR-019 — P3.3 privacy-safe application diagnostics

- Date: 2026-08-22
- Status: Accepted
- Context: Release hardening needs useful failure visibility without leaking
  event identifiers, exception payloads, private notes, or user metadata.
- Decision: Application diagnostics may record stable operation names and
  exception type names only. Raw exception objects and user-derived values are
  excluded from dashboard failure logs; user-visible fallback behavior remains
  unchanged.
- Consequences and trade-offs: Logs provide less stack detail, but remain safe
  for local/device diagnostics and cannot expose sensitive event content through
  exception messages.

## ADR-021 — P3.4 visual design system and local theme preference

- Date: 2026-08-22
- Status: Accepted
- Context: The initial MAUI presentation was functional but did not meet the approved Niko visual direction or provide a deliberate Light/Dark choice.
- Decision: Use an internal MAUI design system based on resource tokens, reusable card and typography styles, local SVG/PNG assets, and the platform `Preferences` store for the presentation-only `System`/`Light`/`Dark` choice. Dashboard, Profile, and Settings consume these resources without moving domain calculations out of Core.
- Alternatives considered: A third-party skin/UI dependency (rejected for the first pass because it adds package, licensing, and upgrade risk), and storing the theme as health/profile domain data (rejected because appearance is not domain data).
- Consequences and trade-offs: The preference survives restart locally but is intentionally separate from SQLite profile data. Local assets keep the app offline-capable. More illustrations or motion libraries require a dependency review before adoption.

## ADR-022 — visual navigation and milestone Island

- Date: 2026-08-22
- Status: Accepted
- Context: The approved product direction needs accessible Home, Log, Battle, Island, and Profile destinations without creating artificial progress data.
- Decision: The MAUI Shell exposes five localized destinations. Island is a presentation-only visual of the existing `DashboardUseCase` snapshot; its illustration is local and it shows only real streak/milestone aggregates. Battle remains backed by the existing Craving Battle use case. No XP, rewards economy, private event details, or new persistence are introduced.
- Consequences and trade-offs: The app gains clearer navigation and richer local visuals while preserving offline behavior. A future gamification system requires a separate Core product decision and tests.

## ADR-023 — P3.5 Recovery Journey visual progression

- Date: 2026-08-22
- Status: Accepted
- Context: The initial Island used one static illustration and did not make real recovery progress visible.
- Decision: Core maps the existing, approximate `RecoverySnapshot` to four stable visual Journey stages. MAUI selects only local stage assets and localized copy from that Core result. No XP, reward economy, storage, raw events, medical diagnosis, or external service is added.
- Consequences and trade-offs: Visual growth is available offline and updates when the existing recovery snapshot changes. Stage artwork is motivational rather than medical evidence; the existing recovery disclaimer remains required.

## ADR-024 — Local export and device-confirmed data erasure

- Date: 2026-08-23
- Status: Accepted
- Context: Users need portable local copies and a clear way to erase Niko data without weakening privacy or requiring a product-specific password.
- Decision: SQLite data is exported only to a user-selected local share path. Erasure is transactional and clears user-owned rows while retaining the database schema; the MAUI flow first shows a clear warning, then requires Android's native secure device credential. Missing or cancelled device confirmation fails closed. Widget refresh follows erasure.
- Consequences and trade-offs: Exported JSON can contain the user's own local data and must be shared intentionally. The app never records the device PIN/pattern/password, and a device without a secure lock cannot erase data through this screen.

## ADR-025 — Aggregate daily savings feedback

- Date: 2026-08-23
- Status: Accepted
- Context: Users need immediate, understandable financial feedback after recording a resisted cigarette, without exposing raw event history in the widget.
- Decision: `DashboardUseCase` exposes the optional aggregate daily savings and the effective price per resisted cigarette. Dashboard shows the daily aggregate; the widget shows both the daily aggregate and the optional per-cigarette value. Both values come only from Core summaries and the persisted profile price/currency.
- Consequences and trade-offs: A price is required; otherwise the UI safely displays its existing unavailable state. The widget receives no event IDs, timestamps, notes, location, or context, and the additive optional fields remain backward-compatible for companions.

## ADR-026 — Presentation-only reduced motion

- Date: 2026-08-23
- Status: Accepted
- Context: Progress animation must not prevent users who prefer less motion from using Niko comfortably.
- Decision: Persist a local presentation preference for reduced motion. The Dashboard progress bar uses its final Core-provided value directly when enabled; it otherwise keeps the existing short animation.
- Consequences and trade-offs: The preference is offline and separate from profile/health data. It does not change progress, timestamps, event handling, or widget behavior.

### ADR-027 — Island daily activity and cumulative savings aggregate

- Context: The Island page needed a privacy-safe daily view of smoked cigarettes and money saved, plus a cumulative total from the quit date.
- Decision: Core calculates one aggregate report per local calendar day from valid persisted `Smoked`/`Resisted` events. Daily savings use the effective price per cigarette and resisted events; the cumulative value is the sum of those daily values. MAUI receives only the aggregate report and never reads or stores raw events independently.
- Constraints: Future, duplicate, deleted, and out-of-range events are excluded. Day boundaries use the injected local timezone. Missing quit date or price produces an empty/unavailable savings result. No network or new persistence is introduced.

## ADR-001 — Shared Core

- Status: Accepted
- Decision: Domain models and use cases live in the shared Core.
- Rationale: Consistent behavior across mobile, wearables, widgets, and tests.
- Consequence: Platforms are adapters; independent domain logic is forbidden.

## ADR-002 — Offline-first event logging

- Status: Accepted
- Decision: Logs are stored locally first and synchronized through a queue.
- Rationale: QuickLog cannot depend on network availability; retry and idempotency are required.
- Consequence: Event IDs, sync status, migrations, and conflict policy must remain stable.

## ADR-003 — Localization from day one

- Status: Accepted
- Decision: Use resource/key-based localization with fallback and RTL/LTR support.
- Rationale: Support 15 languages without broad rewrites.
- Consequence: UI must not depend on source-language length, direction, or formatting rules.

## ADR-004 — Privacy-first AI

- Status: Accepted
- Decision: AI is opt-in, minimum-data, disableable, and non-diagnostic.
- Rationale: Behavioral and health-related data is sensitive; trust is a product requirement.
- Consequence: Guardrails, auditability, and a non-AI fallback are required.

## ADR-005 — Local storage driver

- Date: 2026-08-19
- Status: Accepted
- Context: Need a durable, offline-first local store behind a Core abstraction without heavy dependencies.
- Decision: Use `Microsoft.Data.Sqlite` behind the Core `ILocalStore` interface. Hand-written versioned schema and migrations; no ORM.
- Alternatives considered: EF Core (heavier, more dependencies), sqlite-net-pcl (unnecessary abstraction).
- Consequences and trade-offs: Full SQL control and minimal dependencies; manual migration maintenance. Storage driver is swappable behind `ILocalStore`.

## ADR-006 — Schema and event-contract versioning

- Date: 2026-08-19
- Status: Accepted
- Context: Events and local schema must remain compatible as the app evolves and across platforms.
- Decision: Version the local schema (`schema_meta`) and the serialized event contract. Migrations are additive and backward-compatible; destructive changes require a migration path and consent.
- Alternatives considered: Unversioned schema (rejected: unsafe).
- Consequences and trade-offs: Slightly more maintenance; guaranteed safe upgrades and cross-platform contract stability.

## ADR-007 — Sync contract and idempotency

- Date: 2026-08-19
- Status: Accepted
- Context: No backend exists yet, but sync must be designed to be idempotent, retryable, and resilient.
- Decision: Define `ISyncTransport` in Core. `LogEvent.EventId` is the idempotency key; the server must dedupe by event id. An outbox queue (`SyncQueue`) drains `Pending`/`Failed` events with exponential backoff. In Phase 0 a `NoopSyncTransport` keeps events in the local queue; a real transport is added later.
- Alternatives considered: Real transport now (rejected: no backend specified).
- Consequences and trade-offs: Events stay offline-safe; adding a real transport later requires no Core changes.

## ADR-008 — Localization resource mechanism

- Date: 2026-08-19
- Status: Accepted
- Context: Need resx-based localization with fallback and missing-key reporting across platforms.
- Decision: Use `.resx` satellite resources with the standard .NET `ResourceManager`. Core exposes only stable keys and structured parameters; the MAUI platform adapter resolves strings with culture fallback (exact → parent → neutral) and logs missing keys.
- Alternatives considered: JSON resource files (rejected: requirement is resx).
- Consequences and trade-offs: Adding a language is a new resx file with no domain-code changes.

## ADR-009 — Phase 0 scope (no backend)

- Date: 2026-08-19
- Status: Accepted
- Context: Phase 0 must establish a reliable offline foundation before any network feature.
- Decision: No real sync provider, AI, or Trigger Analysis in Phase 0. Sync is contract + local outbox only.
- Alternatives considered: Implementing a backend in Phase 0 (rejected: out of scope, no endpoint defined).
- Consequences and trade-offs: Foundation is testable and offline-safe; network features are deferred to later phases.

## ADR-010 — Widget and wearable companion contracts

- Date: 2026-08-19
- Status: Accepted
- Context: Widgets and wearable companions must interact with the shared Core without duplicating domain logic or becoming a parallel source of truth.
- Decision: Define versioned, platform-neutral serialized contracts (`CompanionMessage` with `ContractVersion`, `MessageId`, `Source`, `MessageType`, `Payload`) for QuickLog (Smoked/Resisted/Craving), progress summary, streak/milestone summary, and sync status. All messages route through the shared `CompanionUseCase` in Core. Adapter interfaces (`ICompanionAdapter`) are defined in Core; native widget/wearable implementations are deferred.
- Alternatives considered: Full native companion projects now (rejected: scope for Phase 1.7 is contracts only); embedding domain logic in companions (rejected: violates architecture).
- Consequences and trade-offs: Offline-first and idempotent (`MessageId` dedup; `EventId` in Core). `UnsupportedVersion`, `DuplicateEvent`, `MalformedPayload`, and `InvalidSource` produce safe structured failures. The in-memory `IProcessedMessageStore` provides session idempotency; a persistent store may be added later without changing the contract.

## Decision template

## ADR-011 — Profile extension and persisted locale
- Date: 2026-08-20
- Status: Accepted
- Context: P1.8 needs an offline Profile/Settings hub with optional identity fields and a language preference that survives restart.
- Decision: Extend the existing `user_profile` row with nullable `display_name` and a versioned `avatar_id`; reuse `preferred_locale` for language persistence. Locale selection is limited to the configured catalog, with English fallback for locales without complete resources. Language changes remain local and do not alter domain event data.
- Alternatives considered: A separate preferences table (rejected: duplicates the existing profile persistence boundary); platform-specific language storage (rejected: would break cross-platform consistency).
- Consequences and trade-offs: Migration V6 is additive and backward-compatible. Only four locales are fully translated in this release; other configured locales are explicitly marked fallback.


```text
## ADR-XXX — Title
- Date:
- Status: Proposed | Accepted | Superseded
- Context:
- Decision:
- Alternatives considered:
- Consequences and trade-offs:
```
