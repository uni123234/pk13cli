namespace Project.Core.Characters.Models;

/// <summary>
/// Domain model for an equipment item that modifies character attributes.
/// </summary>
public class Item
{
    public int Id { get; }
    public string Name { get; }
    public double HpMod { get; }
    public double PowerMod { get; }

    public Item(int id, string name, double hpMod, double powerMod)
    {
        Id = id;
        Name = name;
        HpMod = hpMod;
        PowerMod = powerMod;
    }
}
