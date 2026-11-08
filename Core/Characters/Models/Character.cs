namespace Project.Core.Characters.Models;

/// <summary>
/// Domain model for an active character participating in gameplay and interactions.
/// </summary>
public class Character
{
    public int Id { get; }
    public string Name { get; set; }
    public int BaseHealth { get; }
    public int BasePower { get; }
    public double BaseResistance { get; }

    public int CurrentHealth { get; private set; }
    public List<Item> ActiveItems { get; } = [];
    public List<Ability> AvailableAbilities { get; } = [];

    public int Health => Math.Max(0, CurrentHealth);
    public int MaxHealth => (int)Math.Round(BaseHealth + ActiveItems.Sum(i => i.HpMod));
    public int Power => (int)Math.Round(BasePower + ActiveItems.Sum(i => i.PowerMod));
    public double Resistance => Math.Clamp(BaseResistance, 0.0, 0.95);
    public bool IsAlive => CurrentHealth > 0;

    public Character(int id, string name, int health, int power, double resistance)
    {
        Id = id;
        Name = name;
        BaseHealth = health;
        BasePower = power;
        BaseResistance = resistance;
        CurrentHealth = health;
    }

    public void AddItem(Item item)
    {
        ActiveItems.Add(item);
        // If HP mod increases max health, adjust current health proportionally if needed
    }

    public bool RemoveItem(int itemId)
    {
        return ActiveItems.RemoveAll(i => i.Id == itemId) > 0;
    }

    public void AddAbility(Ability ability)
    {
        AvailableAbilities.Add(ability);
    }

    public bool RemoveAbility(int abilityId)
    {
        return AvailableAbilities.RemoveAll(a => a.Id == abilityId) > 0;
    }

    public int TakeDamage(int rawDamage)
    {
        var effective = (int)Math.Round(rawDamage * (1.0 - Resistance));
        effective = Math.Max(1, effective);
        CurrentHealth = Math.Max(0, CurrentHealth - effective);
        return effective;
    }

    public int ReceiveHealing(int amount)
    {
        var before = CurrentHealth;
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        return CurrentHealth - before;
    }

    public void SetCurrentHealth(int health)
    {
        CurrentHealth = Math.Clamp(health, 0, MaxHealth);
    }
}
