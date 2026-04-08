# Capisoft AI Transcribe App Monorepo

Speech-to-text API and clients using [**faster-whisper**](https://github.com/SYSTRAN/faster-whisper) (SYSTRAN) for transcription and translation. Production-ready monorepo with clean architecture:

**API:**
- `apps/api`: FastAPI backend for Whisper transcription and translation.

**Libraries:**
- `libraries/FastWhisper.Client` *(submodule)*: .NET client library for API integration. [![NuGet](https://img.shields.io/nuget/v/FastWhisper.Client.svg)](https://www.nuget.org/packages/FastWhisper.Client/)

**Examples:**
- `examples/console-ui`: Python console recorder that sends audio to API.
- `examples/maui`: .NET MAUI client (Windows, Android, iOS, MacCatalyst).

**Infrastructure:**
- `infra/docker`: Container build and compose files for API deployment.

> **Text-to-speech (sibling stack):** [**OmniVoiceApi**](https://github.com/capisoft-lib/OmniVoiceApi) — same idea (HTTP API, Docker, clients), for text-to-speech (voice cloning, streaming).

## Repository Layout

```text
apps/
  api/
    app/
    requirements-api.txt
    scripts/install.ps1
libraries/
  FastWhisper.Client/  (submodule → capisoft-lib/FastWhisperApi.Client)
    src/FastWhisper.Client/
    examples/FastWhisper.Client.Example/
examples/
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
```

## Quick Start

### 1) API (local Python)

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install torch==2.6.* torchvision torchaudio --index-url https://download.pytorch.org/whl/cu124
# Windows + CUDA 12.4: optional Flash Attention 2 wheel (faster GPU inference)
# Wheel is not in this repo; get it from Hugging Face (see link below), or:
pip install "https://huggingface.co/lldacing/flash-attention-windows-wheel/resolve/main/flash_attn-2.7.4%2Bcu124torch2.6.0cxx11abiFALSE-cp311-cp311-win_amd64.whl"
pip install -r apps/api/requirements-api.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000 --app-dir apps/api
```

Pre-built **Flash Attention 2** wheels for Windows (Python 3.11, CUDA 12.4, PyTorch 2.6, etc.) are available at [lldacing/flash-attention-windows-wheel](https://huggingface.co/lldacing/flash-attention-windows-wheel/tree/main). Without this wheel, the API uses PyTorch SDPA.

For full API usage, parameters, and examples, see [apps/api/README-API.md](apps/api/README-API.md).

### 2) Console UI Example

```powershell
.\.venv\Scripts\Activate.ps1
pip install -r examples/console-ui/requirements-recorder.txt
python examples/console-ui/record_and_transcribe.py --url http://127.0.0.1:8000
```

### 3) C# Client Library

The C# client is maintained in a separate repository as a submodule. Initialize it first:

```bash
git submodule update --init --recursive
```

Build and run the example:

```bash
dotnet build libraries/FastWhisper.Client/FastWhisper.Client.slnx
dotnet run --project libraries/FastWhisper.Client/examples/FastWhisper.Client.Example/FastWhisper.Client.Example.csproj
```

For detailed usage, see the [FastWhisper.Client repository](https://github.com/capisoft-lib/FastWhisperApi.Client).

**Install via NuGet:**

[![NuGet](https://img.shields.io/nuget/v/FastWhisper.Client.svg)](https://www.nuget.org/packages/FastWhisper.Client/)

```bash
dotnet add package FastWhisper.Client
```

**Links:**
- 📦 [NuGet Package](https://www.nuget.org/packages/FastWhisper.Client/)
- 📖 [Documentation](https://github.com/capisoft-lib/FastWhisperApi.Client)
- 🔧 [Source Code](https://github.com/capisoft-lib/FastWhisperApi.Client)

### 4) MAUI App Example

Open:

- `examples/maui/Capisoft.AI.TranscribeApp/FastWhisperMaui.sln`

Build targets:

- Windows: `net10.0-windows10.0.19041.0` ✅ Tested
- Android: `net10.0-android` ✅ Tested
- iOS: `net10.0-ios` ⚠️ Not tested
- MacCatalyst: `net10.0-maccatalyst` ⚠️ Not tested

### 5) Docker API

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

## Performance

Tested on NVIDIA GPUs **3060 Ti**, **3080**, and **3090** with very good performance thanks to the [faster-whisper](https://github.com/SYSTRAN/faster-whisper) engine.

## Environment

Copy `.env.example` to `.env` and set values as needed.

## Credits

- [faster-whisper](https://github.com/SYSTRAN/faster-whisper) — Fast Whisper inference (SYSTRAN).

## Security and Contribution

- Security policy: `SECURITY.md`
- Contribution guide: `CONTRIBUTING.md`
- Changelog: `CHANGELOG.md`
