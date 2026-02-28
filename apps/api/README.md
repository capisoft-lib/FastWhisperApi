# Fast Whisper API

FastAPI backend that loads Whisper once at startup and serves transcription/translation endpoints. See [faster-whisper](https://github.com/SYSTRAN/faster-whisper) (SYSTRAN).

## Install

```powershell
cd <repo-root>
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r apps/api/requirements-api.txt
```

## Run

```powershell
uvicorn app.main:app --host 0.0.0.0 --port 8000 --app-dir apps/api
```

## Endpoints

- `GET /health`
- `POST /transcribe`
- `POST /translate`

See `apps/api/README-API.md` for extended examples.
