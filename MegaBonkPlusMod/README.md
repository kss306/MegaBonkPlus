# MegaBonkPlus

**MegaBonkPlus** is a BepInEx mod for Megabonk that adds a powerful web dashboard to control, analyze, and modify the game in real-time.

Monitor your character, spawn items, teleport across the map, and manage your inventory—all from a modern web interface running in your browser.

## ✨ Features

* **Web Dashboard:** A full-featured frontend application (HTML/CSS/JS) served by a local server.
* **Live Game Stats:** Track your character's stats (gold, level, etc.) in real-time.
* **Inventory Management:** Add, remove, or equip weapons and tomes.
* **Action System:** Execute a wide variety of in-game actions:
    * **Gameplay:** Add gold/levels, kill all enemies, absorb all XP, auto-restart runs.
    * **Teleportation:** Teleport to specific coordinates or to the nearest object of interest.
    * **Item Spawner:** Spawn any in-game item.
* **Interactive Minimap:** A live minimap in the web dashboard showing your position, enemies, and key objects.
* **Hotkey Manager:** Configure in-game hotkeys for your favorite actions directly from the web UI.

## 🚀 Installation

Ensure [BepInExPack_IL2CPP](https://bepinex.dev/) (Version 6) is installed for your game.

1.  Compile this project or download the latest release.
2.  Place **both** `MegaBonkPlusMod.dll` and its required dependency `BonkersLib.dll` into your `BepInEx/plugins` folder.
3.  Start the game once to allow the mod to generate its configuration files.

## 🎮 Usage

1.  Start your game with the mod installed.
2.  The mod will automatically start a local web server in the background.
3.  Open your preferred web browser (e.g., Chrome, Firefox).
4.  Navigate to the following address: **[http://localhost:8080](http://localhost:8080)**
5.  The web dashboard should load and connect to your game.

> **Note:** The port `8080` is the default. This can be changed in the mod's configuration file.

## 🛠️ Technical Overview

This mod consists of the following components:

* **Infrastructure:** Starts an `HttpServer` (using `HttpListener`) to handle web requests.
* **Controllers:** Defines API endpoints (e.g., `/api/action/...`, `/api/gamestate`) to receive requests from the frontend.
* **Actions:** Contains the specific logic for all executable cheats/actions (e.g., `GoldAction`, `TeleportAction`).
* **GameLogic:** Contains "Trackers" that poll the game state (player position, enemies, etc.) and provide this data to the frontend.
* **Frontend:** Contains all the static web files (`index.html`, `js/app.js`, `css/...`) that are served by the `HttpServer`.

## ❗ Dependencies

* **BonkersLib:** This mod relies on `BonkersLib.dll`, which provides the core API for interacting with the game.

## 🤝 Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

[MIT LICENSE](https://github.com/kss306/MegaBonkPlus/blob/master/LICENSE)