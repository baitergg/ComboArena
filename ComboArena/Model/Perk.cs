using ComboArena.Model.Abilities;
using System;

namespace ComboArena.Model
{
    /// <summary>
    /// Тип перка — улучшения, которое игрок может выбрать.
    /// </summary>
    public enum PerkType
    {
        /// <summary>Увеличение урона атаки на 20%.</summary>
        DamageUp,

        /// <summary>Уменьшение кулдауна атаки на 20%.</summary>
        AttackSpeedUp,

        /// <summary>Увеличение максимального здоровья на 30%.</summary>
        MaxHealthUp,

        /// <summary>Увеличение скорости передвижения на 15%.</summary>
        SpeedUp,

        /// <summary>Увеличение дальности атаки на 25%.</summary>
        RangeUp,

        /// <summary>Вампиризм: 10% урона восстанавливается как HP.</summary>
        Vampirism,

        /// <summary>Увеличение получаемого опыта на 50%.</summary>
        ExperienceBoost,

        /// <summary>Огненный взрыв (AoE урон вокруг игрока).</summary>
        AbilityFireBurst,

        /// <summary>Щит (временная неуязвимость + отталкивание врагов).</summary>
        AbilityBarrier,

        /// <summary>Увеличение радиуса FireBurst на 30.</summary>
        FireBurstRadiusUp,

        /// <summary>Увеличение урона FireBurst на 15.</summary>
        FireBurstDamageUp,

        /// <summary>Уменьшение кулдауна FireBurst на 20%.</summary>
        FireBurstCooldownDown,

        /// <summary>Увеличение длительности Barrier на 1 секунду.</summary>
        BarrierDurationUp,

        /// <summary>Уменьшение кулдауна Barrier на 20%.</summary>
        BarrierCooldownDown
    }

    /// <summary>
    /// Перк - улучшение, которое игрок выбирает каждые 3 уровня.
    /// Содержит название, описание и логику применения к игроку.
    /// </summary>
    public class Perk
    {
        /// <summary>Название перка (например, "Damage Up").</summary>
        public string Name { get; }

        /// <summary>Описание перка (например, "Increase attack damage by 20%").</summary>
        public string Description { get; }

        /// <summary>Тип перка, определяющий его эффект.</summary>
        public PerkType Type { get; }

        /// <summary>
        /// Создаёт перк указанного типа с автоматическим названием и описанием.
        /// </summary>
        public Perk(PerkType type)
        {
            Type = type;
            (Name, Description) = GetPerkInfo(type);
        }

        /// <summary>
        /// Применяет эффект перка к указанному игроку.
        /// В зависимости от типа перка изменяет характеристики игрока,
        /// добавляет способность или улучшает существующую.
        /// </summary>
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

                case PerkType.AbilityFireBurst:
                    player.SetAbility(new FireBurst());
                    break;

                case PerkType.AbilityBarrier:
                    player.SetAbility(new Barrier());
                    break;

                case PerkType.FireBurstRadiusUp:
                    if (player.Ability is FireBurst fb)
                        fb.UpgradeRadius(30f);
                    break;

                case PerkType.FireBurstDamageUp:
                    if (player.Ability is FireBurst fbDmg)
                        fbDmg.UpgradeDamage(15f);
                    break;

                case PerkType.FireBurstCooldownDown:
                    if (player.Ability is FireBurst fbCd)
                        fbCd.UpgradeCooldown(0.8f);
                    break;

                case PerkType.BarrierDurationUp:
                    if (player.Ability is Barrier b)
                        b.UpgradeDuration(1f);
                    break;

                case PerkType.BarrierCooldownDown:
                    if (player.Ability is Barrier bCd)
                        bCd.UpgradeCooldown(0.8f);
                    break;
            }
        }

        /// <summary>
        /// Возвращает название и описание для указанного типа перка.
        /// </summary>
        /// <param name="type">Тип перка.</param>
        /// <returns>Кортеж (Name, Description).</returns>
        private (string Name, string Description) GetPerkInfo(PerkType type)
        {
            return type switch
            {
                PerkType.DamageUp => ("Damage Up", "Increase attack damage by 20%"),
                PerkType.AttackSpeedUp => ("Attack Speed", "Reduce attack cooldown by 20%"),
                PerkType.MaxHealthUp => ("Max Health", "Increase max health by 30%"),
                PerkType.SpeedUp => ("Speed Up", "Increase movement speed by 15%"),
                PerkType.RangeUp => ("Range Up", "Increase attack range by 25%"),
                PerkType.Vampirism => ("Vampirism", "Heal for 10% of damage dealt"),
                PerkType.ExperienceBoost => ("Experience Boost", "Gain 50% more experience"),
                PerkType.AbilityFireBurst => ("Fire Burst", "Unleash a fiery explosion dealing 25 AoE damage (E key)"),
                PerkType.AbilityBarrier => ("Barrier", "Become invulnerable for 2.5s and push enemies away (E key)"),
                PerkType.FireBurstRadiusUp => ("Fire Burst: Radius+", "Increase Fire Burst radius by 30"),
                PerkType.FireBurstDamageUp => ("Fire Burst: Damage+", "Increase Fire Burst damage by 15"),
                PerkType.FireBurstCooldownDown => ("Fire Burst: Faster", "Reduce Fire Burst cooldown by 20%"),
                PerkType.BarrierDurationUp => ("Barrier: Longer", "Increase Barrier duration by 1s"),
                PerkType.BarrierCooldownDown => ("Barrier: Faster", "Reduce Barrier cooldown by 20%"),
                _ => ("", "")
            };
        }
    }
}
