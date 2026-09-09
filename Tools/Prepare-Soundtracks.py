"""Analyze user originals without changing them; render explicitly selected loop derivatives."""
import hashlib
import json
import pathlib
import subprocess
import numpy as np
import librosa
import imageio_ffmpeg
import soundfile as sf
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
FFMPEG = imageio_ffmpeg.get_ffmpeg_exe()
SOURCES = [pathlib.Path.home() / "Downloads" / name for name in [
    "Echoes of a Fading Summer - Track 2 - Treblo.mp3",
    "Echoes of a Fading Summer - Track 2 - Treblo (1).mp3",
    "Echoes of a Fading Summer - Track 2 - Treblo (2).mp3"]]


def decode(path, rate=22050, channels=1):
    data = subprocess.check_output([FFMPEG, "-v", "error", "-i", str(path),
        "-f", "f32le", "-ar", str(rate), "-ac", str(channels), "-"])
    return np.frombuffer(data, np.float32).reshape(-1, channels)


def loudness(path):
    result = subprocess.run([FFMPEG, "-hide_banner", "-i", str(path), "-af",
        "loudnorm=I=-22:TP=-2:LRA=12:print_format=json", "-f", "null", "-"],
        capture_output=True, text=True, check=True)
    return json.JSONDecoder().raw_decode(result.stderr[result.stderr.rfind("{"):])[0]


def analyze():
    records = []
    for i, path in enumerate(SOURCES):
        samples = decode(path)[:, 0]
        tempo, beats = librosa.beat.beat_track(y=samples, sr=22050, units="time")
        windows = [samples[j:j+22050*10] for j in range(0, len(samples), 22050*10)]
        record = dict(index=i, path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
            seconds=len(samples)/22050, estimatedBpm=float(np.asarray(tempo).item()),
            beats=beats.tolist(), loudness=loudness(path),
            rmsDb10Seconds=[round(float(20*np.log10(np.sqrt(np.mean(w*w))+1e-9)), 2) for w in windows],
            centroidHz=float(np.mean(librosa.feature.spectral_centroid(y=samples, sr=22050))))
        records.append(record)
        print({key: value for key, value in record.items() if key != "beats"}, flush=True)
    target = ROOT / "Docs/Audio150/analysis.json"
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(json.dumps(records, indent=2))


def render():
    records = json.loads((ROOT / "Docs/Audio150/analysis.json").read_text())
    output = ROOT / "Assets/_Game/Audio/Music"
    output.mkdir(parents=True, exist_ok=True)
    manifest = []
    for index, name, desired, count in [(0, "Road", 18, 128), (1, "Menu", 154, 0), (2, "Momentum", 16, 192)]:
        record = records[index]
        if count:
            beats = np.array(record["beats"])
            start_index = int(np.searchsorted(beats, desired))
            start, end = beats[start_index], beats[start_index+count]
            overlap = beats[start_index+count+4] - end
        else:
            start, end, overlap = 154., 210., 4.
        rate = 44100
        source = decode(SOURCES[index], rate, 2)
        start_sample, length, fade = int(start*rate), int((end-start)*rate), int(overlap*rate)
        segment = source[start_sample:start_sample+length+fade]
        phase = np.linspace(0, 1, fade, endpoint=False)[:, None]
        joined = np.concatenate([segment[fade:length],
            segment[length:length+fade]*(1-phase) + segment[:fade]*phase]).astype(np.float32)
        path = output / (name + "Loop.wav")
        staging = ROOT / "Logs" / (name + "-raw-loop.wav")
        sf.write(staging, joined, rate, subtype="PCM_24")
        measured = loudness(staging)
        gain_db = min(-22-float(measured["input_i"]), -2-float(measured["input_tp"]))
        joined *= 10**(gain_db/20)
        sf.write(path, joined, rate, subtype="PCM_24")
        manifest.append(dict(name=name, sourceIndex=index, sourceSha256=record["sha256"],
            sourceStartSeconds=float(start), sourceEndSeconds=float(end), crossfadeSeconds=float(overlap),
            loopSeconds=len(joined)/rate, gainDb=gain_db, loudness=loudness(path),
            boundaryStep=float(np.max(np.abs(joined[-1]-joined[0]))),
            interiorStep99=float(np.percentile(np.abs(np.diff(joined, axis=0)),99)),
            sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    (ROOT / "Docs/Audio150/loops.json").write_text(json.dumps(manifest, indent=2))
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    render() if "--render" in sys.argv else analyze()
