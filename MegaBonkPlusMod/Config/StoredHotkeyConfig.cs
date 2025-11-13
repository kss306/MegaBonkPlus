using System.Collections.Generic;
using MegaBonkPlusMod.Core;

namespace MegaBonkPlusMod.Config;

internal class StoredHotkeyConfig
{
    public bool Enabled { get; set; }
    public List<HotkeyManager.HotkeyDefinition> Hotkeys { get; set; }
}