# Console UI

Terminal recorder that captures microphone audio and sends it to the API.

## Install

```powershell
cd <repo-root>
.\.venv\Scripts\Activate.ps1
pip install -r apps/console-ui/requirements-recorder.txt
```

## Run

```powershell
python apps/console-ui/record_and_transcribe.py --url http://127.0.0.1:8000
```

Optional environment variable:

- `FASTWHISPER_API_URL` default API base URL.
