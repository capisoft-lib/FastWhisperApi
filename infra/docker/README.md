# Docker Deployment

Containerized deployment for the API ([faster-whisper](https://github.com/SYSTRAN/faster-whisper) — SYSTRAN).

## Run

```powershell
docker compose -f infra/docker/docker-compose.yml up --build
```

## Base Image

A pre-built base image is available on Docker Hub: `capitaine/fast-whisper-api`

This base image includes:
- PyTorch 2.6 with CUDA 12.4 support
- ffmpeg for audio processing
- All FastAPI and faster-whisper dependencies
- Optimized build tools (ninja, build-essential)

### Building the Base Image

To build and push the base image to Docker Hub:

```powershell
# Build the base image
docker build -f infra/docker/Dockerfile.base -t capitaine/fast-whisper-api:latest .

# Tag with version
docker tag capitaine/fast-whisper-api:latest capitaine/fast-whisper-api:1.0.0

# Push to Docker Hub
docker push capitaine/fast-whisper-api:latest
docker push capitaine/fast-whisper-api:1.0.0
```

### Using the Public Base Image

To use the public base image in your Dockerfile, replace the base build stage with:

```dockerfile
FROM capitaine/fast-whisper-api:latest

# Copy application code
COPY apps/api/app/ ./app/

# Environment variables
ENV DEVICE_ID=0
ENV BATCH_SIZE=24
ENV FLASH=false

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
```

## API Docs

- Swagger UI: `http://localhost:8000/docs`
- ReDoc: `http://localhost:8000/redoc`
- OpenAPI JSON: `http://localhost:8000/openapi.json`

These match the endpoints and parameters described in [apps/api/README-API.md](../../apps/api/README-API.md) (usage, `timestamp` chunk/word, examples).

## Notes

- Docker build context is repository root.
- API code is copied from `apps/api/app`.
- Runtime port: `8000`.
- The base image speeds up builds by caching dependencies.
