# Long Largo

![Screenshot](logo.jpg)

A MelonLoader mod for **The Long Dark** survival mode.

Long Largo is a mod with a single goal: to enhance your gaming experience by improving the music. **The Long Dark** features a vast array of musical tracks, yet you rarely hear most of them in Survival Mode—and that is precisely what this mod aims to change. Long Largo restores music from the story campaign and integrates it into your playthrough. It does far more than a simple background music player; every track is carefully selected to match specific in-game situations and locations, ensuring the best possible emotional impact. And the mod doesn't stop there: it also allows you to add your own music and customize the settings to your liking.


Short list of features:
* Does not alter the game's gameplay in any way. Do alter immersion in the best way.
* 67 forgotten tracks restored.
* Careful selection of music to match the in-game situation.
* Allows to play exploration music more frequently.
* Ready to go right out of the box. No configuration required!
* Flexible configuration however is presented.
* Supports custom music. With flexible customization!
* Allows to control every musical event.
* ... And you can even disable wolf stalking sound, of course!
  Watch the preview video on Youtube for more information!

## Youtube preview

[![Watch the video](https://img.youtube.com/vi/MQeAOC8LnzA/0.jpg)](https://www.youtube.com/watch?v=MQeAOC8LnzA)

## Exploration music overhaul

The base game contains a vast number of music tracks that exist in the game files but never actually play. One of Long Largo’s primary missions is to bring these tracks back and seamlessly integrate them into the gameplay. While the base game features only one music track per location, Long Largo adds another 52! All of these tracks were composed specifically for the game and fit its atmosphere perfectly. Moreover, each track is tailored to specific in-game situations: some play only during the day, others at night, and some during the aurora. Certain tracks play outdoors, while others are reserved for caves and mines. Some tracks are exclusive to specific regions tied to the events of Wintermute. Furthermore, the vanilla game played music too infrequently—logically due to the rather limited selection of tracks. Since the playlist has become significantly more diverse, the option to play music more often has been added. All of this greatly enhances the enjoyment of the game and occasionally evokes a sense of nostalgia for the main storyline.

## Many events stingers

The vanilla game also features musical stingers. These are short tracks that play in specific situations. They reflect weather changes, the onset of twilight, and your current state. In the vanilla game, they're also fairly limited in variety, despite the existence of unused ones. Long Largo also restores 18 stingers, most of which are used for weather. Different stingers play in different weather conditions, which greatly helps convey the atmosphere of the current situation. This mechanism has proven to be incredibly atmospheric, and further expansion is planned.

## Feeling dangerous

Do you remember when fighting timberwolves wasn't a silent affair? It’s time to refresh that gameplay experience! The combat music for these encounters hasn't just been restored—it now features a dedicated system that adapts the score to the situation, reflecting the many ways you can resolve a confrontation with timberwolves in Survival Mode.

## Create your own atmosphere

Everything discussed so far isn't a decision I’m making unilaterally as the developer. You can also add your own music for absolutely any of the situations mentioned above—and control the atmosphere the game creates for you yourself. It’s all in your hands. Create your own playlists and share them!

## Console commands

Long Largo also supports manual control via in-game console. Here is a list of commands:

* ll_playlist - shows all tracks.
* ll_play - plays track by name.
* ll_play_next - plays next Exploration track.
* ll_stop - stops current track.

## Custom music how-to
Drop soundtracks you want to hear in-game to LongLargo folder in Mods. MP3 and WAV supported. They will be automatically registered and configured in PlaylistInfo.json on next game launch. Long Largo by default treats any musical clip as an exploration clip suitable for day and night. If you want to customize it, open PlaylistInfo.json with text editor or JSON editor and follow the built-in instructions. 

If you want to share your tracks—best solution is to create an assetbundle using Unity designer. Put inside all tracks and PlaylistInfo.json.

## AI Disclaimer

No code or music was developed using AI. All decisions, all code are mine and only mine. All the content you'll get with the mod is created by people. The only exception I make is generating a sketch for the mod logo. Sometimes. And even then, I finish them by hand. They're not part of the mod anyway.

## Known Issues

* Not a bug. MelonLoader 0.7.2 + AudioManager 2.0.5 currently can't load custom soundtracks from folder. A memory-consuming workaround is used as temporary solution. Long Largo will stop being hungry on memory once it fixed. Until then, it is not recommended to upload gigabytes of lossless files to the LongLargo folder, so as not to overflow the RAM.
* Not a bug. Some indoor locations specific to Tales From Far Territory DLC is excluded from supported locations list. It is intended design.
* You might see a message "Long Largo is missing dependencies NAudio v.2.3.0 and NAudio.Core v.2.3.0". It is not a bug, it's just a warning message. All the NAudio dlls will be loaded later. Just ignore it.

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