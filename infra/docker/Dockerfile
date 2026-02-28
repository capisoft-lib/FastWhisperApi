# Linux image: same stack as Windows (PyTorch 2.6, CUDA 12.4, flash_attn).
# Windows as host does not change the image; we build and run a Linux container.
FROM pytorch/pytorch:2.6.0-cuda12.4-cudnn9-devel

ENV DEBIAN_FRONTEND=noninteractive
ENV PYTHONUNBUFFERED=1

# System deps: ffmpeg for loading audio (mp3, etc.), build-essential/ninja for flash-attn if enabled
RUN apt-get update && apt-get install -y --no-install-recommends \
    ffmpeg \
    build-essential \
    ninja-build \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# API deps (torch already in base image)
COPY infra/docker/requirements-docker.txt .
RUN pip install --no-cache-dir -r requirements-docker.txt

# Flash Attention 2: skipped by default (prebuilt wheel can ABI-mismatch with image PyTorch).
# App uses SDPA (FLASH=false). To try flash_attn, uncomment and set ENV FLASH=true.
# RUN pip install --no-cache-dir flash-attn --no-build-isolation

COPY apps/api/app/ ./app/

# Hugging Face cache: use a single dir so a volume can persist the model
ENV HF_HOME=/data/cache
# Default env (override when running)
ENV DEVICE_ID=0
ENV BATCH_SIZE=24
# Use SDPA by default in Docker to avoid flash_attn ABI issues; set FLASH=true if flash_attn works.
ENV FLASH=false

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
