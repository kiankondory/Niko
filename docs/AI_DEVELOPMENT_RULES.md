# Niko — AI Development Rules

## Roles

- **DeepSeek Flash:** routine implementation, bounded refactoring, and local tests.
- **Codex and ChatGPT:** architecture, debugging, code review, safety/privacy review, and quality control.

## Mandatory behavior

- Read `AGENTS.md` and relevant documents before editing.
- Report assumptions, changed files, executed tests, and remaining risks.
- Do not change dependencies, schemas, event contracts, or privacy behavior without justification.
- Do not hide errors by deleting tests, using broad catches, suppressing warnings, or bypassing layers.
- New code files must contain the required Persian header and targeted Persian comments.
- AI Coach output must not provide diagnosis, guaranteed outcomes, prescriptions, or dangerous instructions.

## Review workflow

For sensitive changes, implementation and architecture/security review should be performed separately. Record disagreements, rationale, and trade-offs in `DECISIONS.md`.

