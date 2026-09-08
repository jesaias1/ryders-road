"""Original, deterministic, seamless wind bed; no recordings or third-party samples.
Offline only. Periodic band-limited noise with slow periodic gust modulation.
Requires numpy. The runtime only decodes the resulting mono PCM asset.
"""
from pathlib import Path
import wave
import numpy as np
rate, duration = 22050, 16
n = rate * duration
rng = np.random.default_rng(110)
f = np.fft.rfftfreq(n, 1/rate)
shape = (f / (f + 100)) ** 2 / (1 + (f/700)**2)
spectrum = np.exp(1j*rng.uniform(0, 2*np.pi, len(f))) * shape
spectrum[0] = 0
x = np.fft.irfft(spectrum, n)
t = np.arange(n)/rate
x *= .65 + .2*np.sin(2*np.pi*t/duration) + .10*np.sin(6*np.pi*t/duration)
x *= .45 / np.max(np.abs(x))
out=Path(__file__).resolve().parents[1]/'Assets/_Game/Audio/Clips/Wind110.wav'
with wave.open(str(out),'wb') as wav:
    wav.setparams((1,2,rate,n,'NONE','not compressed'))
    wav.writeframes((x*32767).astype('<i2').tobytes())
print(out)
