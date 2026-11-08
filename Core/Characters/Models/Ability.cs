namespace Project.Core.Characters.Models;

/// <summary>
/// Domain model for an active ability that can deal damage or heal.
/// </summary>
public class Ability
{
    public int Id { get; }
    public string Name { get; }
    public double Damage { get; }
    public double Heal { get; }
    public string Description { get; }

    public Ability(int id, string name, double damage, double heal, string description)
    {
        Id = id;
        Name = name;
        Damage = damage;
        Heal = heal;
        Description = description;
    }
}
