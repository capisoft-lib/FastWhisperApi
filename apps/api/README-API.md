# Fast Whisper API (localhost)

Lightweight local API that **loads the Whisper model once at startup** and keeps it in memory. No Docker. Built on the [**faster-whisper**](https://github.com/SYSTRAN/faster-whisper) project (SYSTRAN).

## One-time setup (torch 2.6 + CUDA 12.4)

Install order matters: PyTorch with CUDA first, then the rest.

```powershell
cd <repo-root>
.\.venv\Scripts\Activate.ps1

# 1) PyTorch 2.6 + CUDA 12.4 (use index that matches your CUDA)
pip install torch==2.6.* torchvision torchaudio --index-url https://download.pytorch.org/whl/cu124

# 2) Optional: Flash Attention 2 (faster). Install from the project wheel (see below).
pip install "flash_attn-2.7.4%2Bcu124torch2.6.0cxx11abiFALSE-cp311-cp311-win_amd64.whl"

# 3) API and Whisper deps
pip install -r apps/api/requirements-api.txt
```

For **CPU-only** or a different CUDA version, see [PyTorch get-started](https://pytorch.org/get-started/locally/).

### Flash Attention 2 wheel (optional)

A prebuilt `flash_attn` wheel is provided in [this repository](https://github.com/capisoft-lib/FastWhisperApi) at the project root (filename: `flash_attn-2.7.4+cu124torch2.6.0cxx11abiFALSE-cp311-cp311-win_amd64.whl`). It was **tested on**:

- **OS:** Windows  
- **Python:** 3.11  
- **CUDA:** 12.4  
- **PyTorch:** 2.6  

This is **one configuration that works** (the one we used). Other combinations (e.g. different Python or CUDA version, or Linux) may work; you can build from source or look for other wheels in the [flash-attention](https://github.com/Dao-AILab/flash-attention) repo. Without this wheel, the API falls back to SDPA (no extra install).

## Attention backends

- **Flash Attention 2**: used when `FLASH=true` (default) and the `flash-attn` package is installed. Fastest on GPU.
- **SDPA (Scaled Dot-Product Attention)**: PyTorch built-in; used when Flash Attn is not available. No extra install.

At startup the app logs which backend is active, e.g. `[Fast Whisper API] device=cuda:0, attention=flash_attention_2`.

## Run the API

```powershell
uvicorn app.main:app --reload --host 127.0.0.1 --port 8000
```

- First startup loads the model (same warmup as your CLI, once).
- Then every request is fast: no reload.

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Check if the server and model are ready |
| POST | `/transcribe` | Upload audio → transcribe → JSON (same shape as `out.json`) |
| POST | `/translate` | Upload audio → translate to English → same JSON shape |

The interactive docs (Swagger UI at `/docs`, ReDoc at `/redoc`) and the OpenAPI spec at `/openapi.json` match these endpoints and parameters.

### Timestamp parameter

The `timestamp` query parameter (used by both `/transcribe` and `/translate`) controls the granularity of timestamps in the response `chunks`:

- **`chunk`** (default) — Segment-level: each chunk is a phrase/segment with one `[start, end]` time range.
- **`word`** — Word-level: each chunk is a single word with its own time range (finer granularity).

### Transcribe

- **Body:** `multipart/form-data` with field `file` = your audio file (e.g. `.mp3`, `.wav`).
- **Query (optional):**  
  - `language` – e.g. `en`, `fr`, or omit for auto-detect.  
  - `timestamp` – `chunk` (default) or `word`.

Example (PowerShell):

```powershell
curl.exe -X POST "http://127.0.0.1:8000/transcribe" -F "file=@/path/to/audio.mp3"
```

Example (Python):

```python
import requests
with open("/path/to/audio.mp3", "rb") as f:
    r = requests.post("http://127.0.0.1:8000/transcribe", files={"file": f})
print(r.json())
```

Response format (same as your `out.json`):

```json
{
  "speakers": [],
  "chunks": [
    { "timestamp": [0, 10.08], "text": " ... " }
  ],
  "text": " ... "
}
```

### Translate

- **Body:** same as transcribe (`file` = audio file).
- **Query (optional):** `timestamp` – `chunk` or `word`.

```powershell
curl.exe -X POST "http://127.0.0.1:8000/translate" -F "file=@/path/to/audio.mp3"
```

## Config (optional)

Environment variables (set before starting the server):

| Variable      | Default | Description              |
|---------------|---------|--------------------------|
| `DEVICE_ID`   | `0`     | GPU device index         |
| `BATCH_SIZE`  | `24`    | Batch size (match CLI)   |
| `FLASH`       | `true`  | Use Flash Attention 2   |

Example:

```powershell
$env:BATCH_SIZE="24"; $env:FLASH="True"; uvicorn app.main:app --host 127.0.0.1 --port 8000
```
