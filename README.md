# Sprocket Penetration Limit Modifier

[![Game](https://img.shields.io/badge/Game-Sprocket-blue)](https://store.steampowered.com/app/1674170/Sprocket/)
[![Mod Loader](https://img.shields.io/badge/Loader-MelonLoader-green)](https://melonwiki.xyz/)

---

这是一个为坦克设计游戏《Sprocket》编写的 MelonLoader 模组。它解除了 Designer 中火炮“穿深”与“口径”滑块的原版限制，允许你突破游戏原有限制，自由设定更为极端的火炮参数。

## 🛠️ 功能特性

* **突破限制**：解锁火炮关键参数的 UI 与底层上限。
    * 穿深 (Penetration)：UI 滑块上限提升至 1000mm（底层解锁至 20000mm）。
    * 口径 (Caliber)：UI 滑块范围扩展至 1mm - 500mm（底层解锁至 1mm - 5000mm）。

* **动态生效**：后台静默监测 UI 状态。无论你是加载全新的坦克图纸，还是在设计中途更换火炮部件，修改都会立刻自动生效。

## 📥 安装方法

1. 确保你已安装最新版本的 [MelonLoader](https://melonwiki.xyz/)。
2. 下载本项目的最新 [Releases](https://github.com/furryaxw/PenetrationMod/releases) 中的 `PenetrationMod.dll`。
3. 将 `.dll` 文件放入游戏根目录的 `Mods` 文件夹中。
4. 启动游戏，进入设计器，尽情调整你的火炮穿深吧！

## 🤝 鸣谢

- **Author**: furryAxw
- **Tools**: Harmony, MelonLoader, Visual Studio 2026

## 📄 License

本项目采用 [GPL-3.0 License](LICENSE.txt) 开源许可。
