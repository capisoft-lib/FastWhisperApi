# Docker Deployment

Containerized deployment for the API ([faster-whisper](https://github.com/SYSTRAN/faster-whisper) — SYSTRAN).

## Run

```powershell
docker compose -f infra/docker/docker-compose.yml up --build
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
