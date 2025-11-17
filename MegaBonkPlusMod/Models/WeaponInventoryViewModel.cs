using System.Collections.Generic;

namespace MegaBonkPlusMod.Models;

public class WeaponInventoryViewModel
{
    public List<WeaponSlotViewModel> weapons { get; set; } = new();
}

public class WeaponSlotViewModel
{
    public string id { get; set; }
    public string name { get; set; }
    public int level { get; set; }
}