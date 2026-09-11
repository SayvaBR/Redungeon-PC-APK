"""Generate the original Lykos transformation howl as a mono PCM wave."""
import math
import random
import struct
import wave
from pathlib import Path

rate = 44_100
duration = 1.55
rng = random.Random(0x1A2B3C)
output = Path(__file__).parents[1] / "Content/Sounds/Characters/Lykos/lykos_howl.wav"
output.parent.mkdir(parents=True, exist_ok=True)
samples = []
phase = 0.0
low_noise = 0.0

for index in range(int(rate * duration)):
    t = index / rate
    attack = min(1.0, t / 0.09)
    release = min(1.0, (duration - t) / 0.36)
    envelope = attack * release
    # A rising call, held peak and falling tail. Vibrato grows in the sustain.
    if t < 0.42:
        fundamental = 235 + 285 * (t / 0.42) ** 0.72
    elif t < 1.02:
        fundamental = 520 - 24 * ((t - 0.42) / 0.60)
    else:
        fundamental = 496 - 190 * ((t - 1.02) / (duration - 1.02))
    vibrato = 1 + (0.008 + 0.012 * min(1, t / 0.6)) * math.sin(2 * math.pi * 5.4 * t)
    phase += 2 * math.pi * fundamental * vibrato / rate
    # Odd harmonics and two broad vocal formants keep it animal rather than synthetic.
    voice = math.sin(phase) + 0.38 * math.sin(2 * phase + 0.5) + 0.20 * math.sin(3 * phase + 1.1)
    formant = 0.16 * math.sin(phase * 1.52 + 0.3) + 0.09 * math.sin(phase * 2.18)
    low_noise = low_noise * 0.94 + (rng.random() * 2 - 1) * 0.06
    breath = low_noise * (0.18 + 0.22 * (1 - envelope))
    value = math.tanh((voice * 0.54 + formant + breath) * 1.25) * envelope
    samples.append(int(max(-1, min(1, value)) * 24_000))

with wave.open(str(output), "wb") as wav:
    wav.setnchannels(1)
    wav.setsampwidth(2)
    wav.setframerate(rate)
    wav.writeframes(b"".join(struct.pack("<h", value) for value in samples))

print(output)
