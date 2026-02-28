# FastWhisper API Base Image

Production-ready base image for speech-to-text transcription API using faster-whisper (SYSTRAN) with CUDA acceleration.

## What's Included

- **PyTorch 2.6** with CUDA 12.4 and cuDNN 9
- **faster-whisper** - Fast Whisper inference engine
- **FastAPI & Uvicorn** - API framework and server
- **ffmpeg** - Audio processing (MP3, WAV, etc.)
- **Build tools** - ninja, build-essential for extensions

## Quick Start

```dockerfile
FROM capitaine/fast-whisper-api:latest

# Copy your application code
COPY app/ ./app/

# Set environment variables
ENV DEVICE_ID=0
ENV BATCH_SIZE=24

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
```

## Features

- ✅ GPU-accelerated transcription with NVIDIA CUDA
- ✅ Multi-language support (98+ languages)
- ✅ Translation capabilities
- ✅ Optimized for production deployments
- ✅ Pre-installed dependencies for fast builds

## Requirements

- Docker with NVIDIA GPU support
- NVIDIA Container Toolkit
- CUDA-compatible GPU

## Source Code

GitHub: https://github.com/capisoft-lib/FastWhisperApi

## License

MIT License
