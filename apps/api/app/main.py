"""
Lightweight local API for insanely-fast-whisper.
Model is loaded once at startup and kept in memory for fast transcription.

Attention backends (automatic):
- Flash Attention 2: used when FLASH=true (default) and the flash-attn package
  is installed and available (faster on GPU).
- SDPA (Scaled Dot-Product Attention): PyTorch built-in; used when Flash Attn
  is not available. No extra install needed.
"""
import os
import tempfile
from contextlib import asynccontextmanager

import torch
from fastapi import FastAPI, File, HTTPException, UploadFile
from transformers import pipeline
from transformers.utils import is_flash_attn_2_available

# Config (env or defaults matching your CLI)
DEVICE_ID = int(os.environ.get("DEVICE_ID", "0"))
BATCH_SIZE = int(os.environ.get("BATCH_SIZE", "24"))
USE_FLASH = os.environ.get("FLASH", "true").lower() in ("true", "1", "yes")

# Pipeline instance, loaded at startup
pipe = None


def _get_device():
    if torch.cuda.is_available():
        return f"cuda:{DEVICE_ID}"
    if hasattr(torch.backends, "mps") and torch.backends.mps.is_available():
        return "mps"
    return "cpu"


def _build_pipeline():
    device = _get_device()
    use_fa2 = USE_FLASH and is_flash_attn_2_available()
    model_kwargs = (
        {"attn_implementation": "flash_attention_2"} if use_fa2 else {"attn_implementation": "sdpa"}
    )
    attn_backend = "flash_attention_2" if use_fa2 else "sdpa (Scaled Dot-Product Attention)"
    print(f"[Fast Whisper API] device={device}, attention={attn_backend}")  # noqa: T201
    return pipeline(
        "automatic-speech-recognition",
        model="openai/whisper-large-v3",
        torch_dtype=torch.float16 if device != "cpu" else torch.float32,
        device=device,
        model_kwargs=model_kwargs,
    )


def _normalize_output(raw):
    """Convert pipeline output to your out.json-style format."""
    chunks = []
    text = raw.get("text") or ""
    for c in raw.get("chunks") or []:
        ts = c.get("timestamp")
        if ts is not None:
            ts = list(ts) if isinstance(ts, (list, tuple)) else [ts, ts]
        else:
            ts = [0.0, 0.0]
        chunks.append({"timestamp": ts, "text": (c.get("text") or "").strip() or " "})
    return {
        "speakers": [],
        "chunks": chunks,
        "text": text.strip() if text else "",
    }


@asynccontextmanager
async def lifespan(app: FastAPI):
    global pipe
    pipe = _build_pipeline()
    yield
    # optional: clear GPU memory on shutdown
    del pipe
    pipe = None
    if torch.cuda.is_available():
        torch.cuda.empty_cache()


app = FastAPI(
    title="Fast Whisper API",
    description="Local transcribe/translate with model kept in memory",
    lifespan=lifespan,
)


@app.get("/health")
def health():
    return {"status": "ok", "model_loaded": pipe is not None}


def _run_pipeline(audio_path: str, task: str, language: str | None, timestamp: str):
    generate_kwargs = {"task": task}
    if language and language.lower() != "none":
        generate_kwargs["language"] = language
    else:
        generate_kwargs["language"] = None

    out = pipe(
        audio_path,
        chunk_length_s=30,
        batch_size=BATCH_SIZE,
        generate_kwargs=generate_kwargs,
        return_timestamps="word" if timestamp == "word" else True,
    )
    return _normalize_output(out)


@app.post("/transcribe")
def transcribe(
    file: UploadFile = File(...),
    language: str | None = None,
    timestamp: str = "chunk",
):
    """Upload an audio file; returns same structure as out.json (chunks + text)."""
    if pipe is None:
        raise HTTPException(status_code=503, detail="Model not loaded yet")
    if timestamp not in ("chunk", "word"):
        timestamp = "chunk"

    suffix = os.path.splitext(file.filename or "audio")[1] or ".mp3"
    try:
        with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as tmp:
            tmp.write(file.file.read())
            tmp_path = tmp.name
        try:
            result = _run_pipeline(tmp_path, "transcribe", language, timestamp)
            return result
        finally:
            try:
                os.unlink(tmp_path)
            except OSError:
                pass
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/translate")
def translate(
    file: UploadFile = File(...),
    timestamp: str = "chunk",
):
    """Upload an audio file; translate to English and return out.json-style output."""
    if pipe is None:
        raise HTTPException(status_code=503, detail="Model not loaded yet")
    if timestamp not in ("chunk", "word"):
        timestamp = "chunk"

    suffix = os.path.splitext(file.filename or "audio")[1] or ".mp3"
    try:
        with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as tmp:
            tmp.write(file.file.read())
            tmp_path = tmp.name
        try:
            result = _run_pipeline(tmp_path, "translate", None, timestamp)
            return result
        finally:
            try:
                os.unlink(tmp_path)
            except OSError:
                pass
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
