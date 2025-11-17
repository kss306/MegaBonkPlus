# BonkersLib

**BonkersLib** is a core shared library for BepInEx mod development targeting the game "Megabonk".

Its primary purpose is to provide a stable API and reusable services to interact with the game's code. It is used as a dependency by other mods, such as `MegaBonkPlusMod`.

## 🛠️ Core Services

This library abstracts away game-specific code into several easy-to-use services, including:

* **`PlayerService`:** For accessing and manipulating player data.
* **`WorldService`:** For interacting with the game world and objects.
* **`GameStateService`:** For retrieving and tracking the overall game state.
* *(And other helpers for managing inventory, items, etc.)*

## 🚀 Installation (For End-Users)

If you are a user of a mod that requires `BonkersLib`:

Ensure [BepInExPack_IL2CPP](https://bepinex.dev/) (Version 6) is installed for your game.

1.  Compile this project or download the latest release.
2.   Place the `BonkersLib.dll` into your `BepInEx/plugins` folder, alongside the mod that needs it.
3.  Start the game once to allow the mod to generate its configuration files.

## 🧑‍💻 Usage (For Developers)

If you are developing a mod that builds on this library:

1.  Add `BonkersLib.dll` as a reference in your C# project.
2.  Ensure your mod declares `BonkersLib.dll` as a dependency (e.g., via BepInEx attributes).
3.  Access the provided services (e.g., `BonkersAPI.PlayerService`).

## 🤝 Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

[MIT LICENSE](https://github.com/kss306/MegaBonkPlus/blob/master/LICENSE)