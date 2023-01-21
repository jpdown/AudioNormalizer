# AudioNormalizer

A Beat Saber mod that adds volume normalization to custom levels.

## Dependencies

All dependencies can be installed from ModAssistant/BeatMods
* SiraUtil v3.0.0+
* SongCore v3.9.5+
* BSML v1.6.3+
* ffmpeg v4.3.0+

## Installation

* Download the latest version of the mod [here](https://github.com/jpdown/AudioNormalizer/releases). The latest dev build can be downloaded from [actions](https://github.com/jpdown/AudioNormalizer/actions).
* Place the dll in your Plugins folder

## Details

* Loudness calculation is done through ffmpeg, using the EBU R128 algorithm.
* The mod hooks into the game's built in loudness correction, so results should be consistent with official songs.
