# Sprocket Penetration Limit Modifier

[中文](README.zh.md) | **English**

[![Game](https://img.shields.io/badge/Game-Sprocket-blue)](https://store.steampowered.com/app/1674170/Sprocket/)
[![Mod Loader](https://img.shields.io/badge/Loader-MelonLoader-green)](https://melonwiki.xyz/)

---

A MelonLoader mod for the tank design game Sprocket. It raises the cannon caliber and penetration limits of the armour testing tool in the designer; the limits can be adjusted from the config page in the in-game mod menu.

## 🛠️ Features

* **Break the limits**: Unlocks both the UI and the underlying limits on key cannon parameters. The defaults are listed below and can be changed from the config page in the in-game mod menu.
    * Penetration: UI slider capped at 1000 mm (underlying limit 20000 mm).
    * Caliber: UI slider range 1 mm - 500 mm (underlying range 1 mm - 5000 mm).
* **Takes effect live**: Silently monitors the UI state in the background. Whether you load a brand-new tank blueprint or swap the cannon part mid-design, your changes apply automatically right away.

## 📥 Installation

1. Make sure you have the latest version of [MelonLoader](https://melonwiki.xyz/) installed.
2. Download `PenetrationMod.dll` from the latest [Releases](https://github.com/furryaxw/PenetrationMod/releases) of this project.
3. Put the `.dll` file into the `Mods` folder in the game root directory.
4. Launch the game, enter the designer, and go wild tuning your cannon's penetration!

## 🤝 Credits

- **Author**: furryAxw
- **Tools**: Harmony, MelonLoader, Visual Studio 2026

## 📄 License

This project is released under the [GPL-3.0 License](LICENSE.txt).
