# Contributing

## Development setup

1. Clone repository.
2. Create Python virtual environment at repo root.
3. Install API/console dependencies as needed.
4. Open MAUI solution from `apps/maui/Capisoft.AI.TranscribeApp/FastWhisperMaui.sln`.

## Code style

- Python: PEP8-compatible style.
- C#: follow SDK defaults and `.editorconfig`.
- Keep changes scoped by app (`apps/api`, `apps/console-ui`, `apps/maui`).

## Pull requests

- Use feature branches.
- Keep commits focused and descriptive.
- Include verification steps (build/run/tests) in PR description.
- Do not commit local artifacts, secrets, recordings, or machine-specific paths.

## Validation before PR

- API starts successfully.
- MAUI app builds for Windows and Android.
- Docker build succeeds with `infra/docker/Dockerfile`.
