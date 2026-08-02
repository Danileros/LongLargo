# Long Largo

![Screenshot](logo.jpg)

A MelonLoader mod for **The Long Dark** survival mode.

Long Largo is a mod with a single goal: to enhance your gaming experience by improving the music. **The Long Dark** features a vast array of musical tracks, yet you rarely hear most of them in Survival Mode—and that is precisely what this mod aims to change. Long Largo restores music from the story campaign and integrates it into your playthrough. It does far more than a simple background music player; every track is carefully selected to match specific in-game situations and locations, ensuring the best possible emotional impact. And the mod doesn't stop there: it also allows you to add your own music and customize the settings to your liking.

* 67 forgotten tracks restored.
* Careful selection of music to match the in-game situation.
* Supports custom music.
* Flexible playlist customization.
* Allows to play exploration music more frequently.
* Flexible setting.
* Allows to control every musical event. You can even disable wolf/bear stalk sound, and much more!

## Custom music how-to
Drop soundtracks you want to hear in-game to LongLargo folder in Mods. MP3 and WAV supported. They will be automatically registered and configured in PlaylistInfo.json on next game launch. Long Largo by default treats any musical clip as an exploration clip suitable for day and night. If you want to customize it, open PlaylistInfo.json with text editor or JSON editor and follow the built-in instructions. 

If you want to share your tracks—best solution is to create an assetbundle using Unity designer. Put inside all tracks and PlaylistInfo.json.

## Known Issues

* Not a bug. MelonLoader 0.7.2 + AudioManager 2.0.5 currently can't load custom soundtracks from folder. A memory-consuming workaround is used as temporary solution. Long Largo will stop being hungry on memory once it fixed. Until then, it is not recommended to upload gigabytes of lossless files to the LongLargo folder, so as not to overflow the RAM.
* TLD bug: Timberwolf combat does not stops in some conditions (like escaping). It may stop LL from playing exploration music. It also the reason why I hadn't restored combat music. You are free to add custom timberwolf combat music, it's fully supported.
* Not a bug. Some indoor locations specific to Tales From Far Territory DLC is excluded from supported locations list. It is intended design.

## Installation

* Common mods installation process is described at [TLD Mods](https://tldmods.net/install.html). Make sure your MelonLoader version matches.
* Install all the dependencies:

- [ModSettings](https://github.com/DigitalzombieTLD/ModSettings/releases)
- [AudioManager](https://github.com/DigitalzombieTLD/AudioManager/releases)


* Install the latest AudioCore plugin (to Plugins folder, not Mods folder!):

- [AudioCore](https://github.com/DigitalzombieTLD/AudioCore/releases)

* Download latest Long Largo version from Releases page.
* Drop archive files to Mods folder in The Long Dark game directory.

## Special Thanks

[DigitalZombie](https://github.com/DigitalzombieTLD/) for creating AudioManager and AudioCore that I depend on.
[Fuar11](https://github.com/Fuar11) for Improved soundtracks mod that I've drawn inspiration from.
Without you, I wouldn't have even started developing Long Largo.