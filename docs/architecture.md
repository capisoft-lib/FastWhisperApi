# Architecture Overview

Speech-to-text is powered by [**faster-whisper**](https://github.com/SYSTRAN/faster-whisper) (SYSTRAN).

## Components

- `apps/api`: FastAPI service that exposes `/health`, `/transcribe`, and `/translate`.
- `apps/console-ui`: microphone recorder and API client for terminal usage.
- `apps/maui`: cross-platform app for Windows/Android/iOS/MacCatalyst.
- `infra/docker`: containerization and compose deployment for API.

## Data Flow

1. Client (console or MAUI) records audio.
2. Client posts multipart audio file to API.
3. API runs Whisper pipeline and returns normalized JSON.
4. MAUI stores local history (SQLite + local audio files).
