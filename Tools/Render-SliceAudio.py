"""Original offline modal contact sounds; deterministic, no third-party samples."""
from pathlib import Path
import wave
import numpy as np

rate = 22050
root = Path(__file__).resolve().parents[1] / 'Assets/_Game/Audio/Clips'
rng = np.random.default_rng(120)

def save(name, samples):
    samples *= .72 / max(1e-6, np.max(np.abs(samples)))
    samples[-400:] *= np.linspace(1, 0, 400)
    with wave.open(str(root / name), 'wb') as wav:
        wav.setparams((1, 2, rate, len(samples), 'NONE', 'not compressed'))
        wav.writeframes((samples * 32767).astype('<i2').tobytes())

t = np.arange(int(rate * 1.8)) / rate
patch = np.zeros_like(t)
# Softly struck, staggered ceramic resonances with inharmonic upper partials.
for onset, base in [(0, 261.63), (.07, 329.63), (.14, 392)]:
    age = np.maximum(0, t - onset)
    for ratio, gain in [(1, .6), (2.01, .21), (3.93, .07), (6.14, .025)]:
        patch += (t >= onset) * gain * np.sin(2*np.pi*base*ratio*age) * (1-np.exp(-age*180)) * np.exp(-age*(2.8+ratio*.6))
save('Patch120.wav', patch)
t = np.arange(int(rate * .48)) / rate
noise = rng.normal(size=len(t))
noise = np.convolve(noise, np.ones(13)/13, mode='same')
# A short pneumatic release with a low contact transient, not a continuous whoosh.
boost = noise * np.sin(np.pi*np.minimum(1,t/.48))**2 * np.exp(-t*7)
boost += .09*np.sin(2*np.pi*(95*t-30*t*t))*(1-np.exp(-t*240))*np.exp(-t*24)
save('Boost120.wav', boost)
