# SoRFramerateLimiter
A BepInEx plugin for Streets of Rogue for limiting framerate when VSync is off.

![alt text](https://github.com/Moonkis/SoRFramerateLimiter/blob/master/preview.png?raw=true)

## Installation

1. Download the x64 version of BepInEx 5.1 [here](https://github.com/BepInEx/BepInEx/releases).
2. Unpack the BepInEx zip into the root folder of Streets of Rogue.
3. Start Streets of Rogue clean once without any mods, this is to make sure BepInEx creates it's necessary folder structure. If you already have other mods installed, skip this step.
4. Exit from the main menu
5. Download the latest release of the FramerateLimiter [here](https://github.com/Moonkis/SoRFramerateLimiter/releases)
6. Go into `BepInEx/Plugins` and unzip the file `SoRFramerateLimiter.dll` from the latest SoRFramerateRelease here.
7. Start Streets of Rogue and enjoy, the setting can be found under the Graphics menu.

## Note
SoRFramerateLimiter stores it's persistent data in the same area as save files under the directory ModConfig. On Windows this is normally `AppData/LocalLow/Streets of Rogue`
