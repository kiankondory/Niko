# Niko — Architecture

## System shape

```text
MAUI Mobile / Wear OS / watchOS / Android Widget / iOS Widget
                         ↓
              Application Use Cases (Core)
                         ↓
             Domain Events + Policies (Core)
                         ↓
 Local Store / Sync Queue / Remote Sync / AI Adapters
```

- Core is independent of UI and platform code.
- Events are durably stored locally before entering the sync queue.
- Event-driven logging covers `Smoked`, `Resisted`, `Craving`, and related events.
- Sync must be idempotent, retryable, observable, and resilient to network loss.
- Widgets and companions must not create parallel storage or business rules.

## Event contract

Every log has a unique `EventId`, timestamp, source, event type, limited metadata, and sync status. Contract changes must be versioned and recorded in `DECISIONS.md`.

## Cross-cutting concerns

Localization, minimal telemetry, encryption, accessibility, feature flags, and error handling must be implemented through shared abstractions and policies.

