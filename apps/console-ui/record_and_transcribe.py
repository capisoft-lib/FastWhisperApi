#!/usr/bin/env python3
"""
Record from the microphone; when you press Enter, stop and save the file.
Check the file, then press Enter again to send it to the Fast Whisper API.
Requires: pip install sounddevice soundfile requests
"""
import argparse
import os
import sys
import threading
import time

import numpy as np
import requests
import sounddevice as sd
import soundfile as sf

SAMPLE_RATE = 16000
CHANNELS = 1
DTYPE = "float32"
API_URL_DEFAULT = os.environ.get("FASTWHISPER_API_URL", "http://127.0.0.1:8000")
DEFAULT_SAVE_PATH = "last_recording.wav"


def main():
    parser = argparse.ArgumentParser(
        description="Record mic → save → press Enter again to send to transcribe API"
    )
    parser.add_argument("--url", default=API_URL_DEFAULT, help="API base URL (default: %(default)s)")
    parser.add_argument("--language", default=None, help="Language code (e.g. en, fr) or omit for auto")
    parser.add_argument(
        "--out",
        default=DEFAULT_SAVE_PATH,
        help="Path to save recording (default: %(default)s)",
    )
    args = parser.parse_args()

    base = args.url.rstrip("/")
    transcribe_url = f"{base}/transcribe"
    save_path = os.path.abspath(args.out)

    print("Recording from microphone. Press Enter to stop and save.", flush=True)
    frames = []

    def callback(indata, _frame_count, _time_info, _status):
        if _status:
            print(_status, file=sys.stderr)
        frames.append(indata.copy())

    stream = sd.InputStream(
        samplerate=SAMPLE_RATE,
        channels=CHANNELS,
        dtype=DTYPE,
        blocksize=1024,
        callback=callback,
    )
    stream.start()

    input()
    stream.stop()
    stream.close()

    if not frames:
        print("No audio recorded.", file=sys.stderr)
        sys.exit(1)

    audio = np.concatenate(frames, axis=0)
    sf.write(save_path, audio, SAMPLE_RATE)
    duration_s = len(audio) / SAMPLE_RATE
    print(f"Saved to {save_path} ({duration_s:.1f}s). Check it, then press Enter to send to API.", flush=True)

    input()

    with open(save_path, "rb") as f:
        t0 = time.perf_counter()
        r = requests.post(
            transcribe_url,
            files={"file": (os.path.basename(save_path), f, "audio/wav")},
            params={"language": args.language} if args.language else None,
            timeout=300,
        )
        elapsed = time.perf_counter() - t0
    r.raise_for_status()
    out = r.json()
    print(f"[API: {elapsed:.2f}s]")
    print(out.get("text", ""))
    for c in out.get("chunks", []):
        ts = c.get("timestamp", [0, 0])
        t = c.get("text", "").strip()
        if t:
            print(f"  [{ts[0]:.1f}s - {ts[1]:.1f}s] {t}")


if __name__ == "__main__":
    main()
