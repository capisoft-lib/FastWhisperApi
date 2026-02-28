# Capisoft AI Transcribe App Monorepo

Speech-to-text API and clients using [**faster-whisper**](https://github.com/SYSTRAN/faster-whisper) (SYSTRAN) for transcription and translation. Production-ready monorepo containing:

- `apps/api`: FastAPI backend for Whisper transcription and translation.
- `apps/console-ui`: Python console recorder that sends audio to API.
- `apps/maui/Capisoft.AI.TranscribeApp`: .NET MAUI client (Windows, Android, iOS, MacCatalyst).
- `infra/docker`: Container build and compose files for API deployment.

## Repository Layout

```text
apps/
  api/
    app/
    requirements-api.txt
    scripts/install.ps1
  console-ui/
    record_and_transcribe.py
    requirements-recorder.txt
  maui/
    Capisoft.AI.TranscribeApp/
infra/
  docker/
    Dockerfile
    docker-compose.yml
    requirements-docker.txt
docs/
```

## Quick Start

### 1) API (local Python)

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install torch==2.6.* torchvision torchaudio --index-url https://download.pytorch.org/whl/cu124
pip install -r apps/api/requirements-api.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000 --app-dir apps/api
```

For full API usage, parameters, and examples, see [apps/api/README-API.md](apps/api/README-API.md).

### 2) Console UI

```powershell
.\.venv\Scripts\Activate.ps1
pip install -r apps/console-ui/requirements-recorder.txt
python apps/console-ui/record_and_transcribe.py --url http://127.0.0.1:8000
```

### 3) MAUI App

Open:

- `apps/maui/Capisoft.AI.TranscribeApp/FastWhisperMaui.sln`

Build targets:

- Windows: `net10.0-windows10.0.19041.0` ✅ Tested
- Android: `net10.0-android` ✅ Tested
- iOS: `net10.0-ios` ⚠️ Not tested
- MacCatalyst: `net10.0-maccatalyst` ⚠️ Not tested

### 4) Docker API

#### Quick Start with Docker Hub Image

Use the pre-built base image from Docker Hub:

```bash
docker pull capitaine/fast-whisper-api:latest
```

Or run with Docker Compose:

```powershell
docker compose -f infra/docker/docker-compose.yml up --build
```

For more information about the Docker deployment and base image, see [infra/docker/README.md](infra/docker/README.md).

**Docker Hub**: [capitaine/fast-whisper-api](https://hub.docker.com/r/capitaine/fast-whisper-api)

## Environment

Copy `.env.example` to `.env` and set values as needed.

## Credits

- [faster-whisper](https://github.com/SYSTRAN/faster-whisper) — Fast Whisper inference (SYSTRAN).

## Security and Contribution

- Security policy: `SECURITY.md`
- Contribution guide: `CONTRIBUTING.md`
- Changelog: `CHANGELOG.md`
