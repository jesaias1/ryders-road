# Original soundtrack integration — 0.15.0

All three requested Downloads files were accessible and analyzed separately using
FFmpeg loudness/PCM analysis and librosa energy, beat and spectral measurements.
They have different hashes, lengths and arrangements. This is signal analysis,
not a claim of subjective listening. Original files were never rewritten.
`analysis.json` contains full provenance, hashes and 10-second energy contours;
`loops.json` records exact selections, gains, final metrics and derivative hashes.

| Source filename suffix | Length | Estimated tempo | Integrated LUFS / true peak | LRA | Choice |
| --- | ---: | ---: | --- | ---: | --- |
| Treblo.mp3 | 176.01 s | 129.2 BPM | -15.17 / -1.02 dBTP | 9.0 LU | Road: default Campaign |
| Treblo (1).mp3 | 226.67 s | 117.5 BPM | -14.64 / -0.76 dBTP | 14.4 LU | Menu and Ancient Abyss |
| Treblo (2).mp3 | 174.48 s | 161.5 BPM | -15.13 / -1.82 dBTP | 13.2 LU | Foundry, Spiral, practice |

Tempo estimates can have half/double-time ambiguity; these are not genre labels.
The unsuffixed source has the lowest average spectral centroid (~616 Hz versus
1237/1305 Hz) and a more restrained full-track dynamic range: a reasonable measured
basis for an unobtrusive default. Its early established section supplies a steady
58.65-second Road loop. Source (1) has a distinct lower-energy coda after about
150 seconds; selecting 154–210 seconds avoids forcing its more variable complete
arrangement into the menu. Source (2)'s faster estimated pulse and early sustained
energy suit the existing movement-focused contexts. These suitability judgments
are inferences from the measurements and remain subject to listening feedback.

## Loop preparation

`Tools/Prepare-Soundtracks.py --render` reproduces the derivatives from the three
specified originals. Road selects 18.36698–77.02059 s with a 1.83438 s overlap;
Momentum selects 16.09143–86.42467 s with a 1.46286 s overlap. These rhythmic overlaps
are four estimated beats. Menu selects 154–210 s with a four-second crossfade.
The tail after each endpoint blends into the segment beginning, then wraps to its
adjacent continuation. There are no inserted silence pads or destructive edits.

All three final WAVs measure **-22.00 LUFS**. Road/Menu/Momentum peaks respectively
measure -8.34/-6.27/-10.28 dBTP. These are constant-gain derivatives, not dynamically
normalized masters. In the FFmpeg metric objects, `input_*` describes the measured
final file; `output_*` is FFmpeg's hypothetical normalization output and is unused.
The boundary sample steps are below each loop's 99th-percentile interior sample
step. This checks excess discontinuity, not perceptual musical seamlessness.

Files are stereo 44.1 kHz PCM24 production assets under Audio/Music, tracked in LFS.
Unity uses streaming Vorbis quality 0.7, preserves stereo, loads in background and
does not preload decoded whole tracks. Only two streaming sources coexist during
the two-second unscaled fade. Same-track Retry/Restore preserves playback position.
Interrupted transitions retain the louder source; application pause pauses audio.
Gameplay pause does not stop atmospheric music. Music has no movement/save authority.

Settings exposes MUSIC independently of MASTER/SFX, default 65% with profile gain
0.85. Master still affects the overall listener. Automated playback checks exercise
an actual loop boundary, mute/unmute, unchanged-context playback, interrupted fades
and pause/resume. Final compressed-loop musical seams, SFX intelligibility, phone
speaker/buds balance and decode cost require physical listening/performance tests.
