using System;
namespace ComboArena.Model
{
    public enum PerkType
    {
        DamageUp,
        AttackSpeedUp,
        MaxHealthUp,
        SpeedUp,
        RangeUp,
        Vampirism,
        ExperienceBoost
    }
    public class Perk
    {
        public string Name { get; }
        public string Description { get; }
        public PerkType Type { get; }
        public Perk(PerkType type)
        {
            Type = type;
            (Name, Description) = GetPerkInfo(type);
        }
        public void Apply(Player player)
        {
            switch (Type)
            {
                case PerkType.DamageUp:
                    player.ModifyAttackDamage(1.2f);
                    break;
                case PerkType.AttackSpeedUp:
                    player.ModifyAttackCooldown(0.8f);
                    break;
                case PerkType.MaxHealthUp:
                    player.ModifyMaxHealth(1.3f);
                    break;
                case PerkType.SpeedUp:
                    player.ModifySpeed(1.15f);
                    break;
                case PerkType.RangeUp:
                    player.ModifyAttackRange(1.25f);
                    break;
                case PerkType.Vampirism:
                    player.AddVampirismPercent(0.1f);
                    break;
                case PerkType.ExperienceBoost:
                    player.AddExperienceMultiplier(1.5f);
                    break;
            }
        }
        private (string Name, string Description) GetPerkInfo(PerkType type)
        {
            return type switch
            {
                PerkType.DamageUp => ("Damage Up", "Increase attack damage by 20%"), // заменить на шрифт с поддерждкой русского
                PerkType.AttackSpeedUp => ("Attack Speed", "Reduce attack cooldown by 20%"),
                PerkType.MaxHealthUp => ("Max Health", "Increase max health by 30%"),
                PerkType.SpeedUp => ("Speed Up", "Increase movement speed by 15%"),
                PerkType.RangeUp => ("Range Up", "Increase attack range by 25%"),
                PerkType.Vampirism => ("Vampirism", "Heal for 10% of damage dealt"),
                PerkType.ExperienceBoost => ("Experience Boost", "Gain 50% more experience"),
                _ => ("Unknown", "")
            };
        }
    }
}
