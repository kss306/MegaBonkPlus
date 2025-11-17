using System.Collections.Generic;

namespace MegaBonkPlusMod.Models;

public class TomeInventoryViewModel
{
    public List<TomeSlotViewModel> tomes { get; set; } = new();
}

public class TomeSlotViewModel
{
    public string id { get; set; }
    public string name { get; set; }
    public int level { get; set; }
}