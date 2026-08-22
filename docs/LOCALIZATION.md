# Niko — Localization Strategy

Niko is designed for at least 15 languages from day one. Adding a language must not require broad code changes.

## Mandatory rules

- All user-facing text uses stable resource/key-based localization; hard-coded UI text is forbidden.
- Keys are stable, descriptive, and independent from translations. Translations live in locale resources.
- Fallback must exist at both language and key level, and missing translations must be reportable.
- RTL languages use real layout direction; do not manually pin text or icons to left/right assumptions.
- Pluralization, gender, word selection, and interpolation use locale-aware mechanisms.
- Dates, times, numbers, percentages, units, and currencies use locale-aware formatters.
- Layouts must tolerate translated text expansion and different font metrics.

## Boundary

Core knows localization keys and structured parameters only. Platform adapters resolve the correct resources. Tests must cover key existence, fallback, RTL/LTR, formatting, and prevention of raw-key display.

## Adding a language

Add locale resources, locale metadata, format/snapshot tests, and native-language review. Do not change domain logic or page structure unless required by the language.

