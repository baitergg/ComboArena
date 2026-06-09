using ComboArena.Model.Abilities;
using System;

namespace ComboArena.Model
{
    /// <summary>
    /// Тип перка - улучшения, которое игрок может выбрать.
    /// </summary>
    public enum PerkType
    {
        DamageUp,

        AttackSpeedUp,

        MaxHealthUp,

        SpeedUp,

        RangeUp,

        Vampirism,

        ExperienceBoost,

        AbilityFireBurst,

        AbilityBarrier,

        FireBurstRadiusUp,

        FireBurstDamageUp,

        FireBurstCooldownDown,

        BarrierDurationUp,

        BarrierCooldownDown
    }

    /// <summary>
    /// Перк - улучшение, которое игрок выбирает каждые 3 уровня.
    /// Содержит название, описание и логику применения к игроку.
    /// </summary>
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

        private (string Name, string Description) GetPerkInfo(PerkType type)
        {
            return type switch
            {
                PerkType.DamageUp => ("Урон +", "Увеличивает урон атаки на 20%"),
                PerkType.AttackSpeedUp => ("Скорость атаки", "Уменьшает перезарядку атаки на 20%"),
                PerkType.MaxHealthUp => ("Макс. здоровье", "Увеличивает макс. здоровье на 30%"),
                PerkType.SpeedUp => ("Скорость", "Увеличивает скорость передвижения на 15%"),
                PerkType.RangeUp => ("Дальность", "Увеличивает дальность атаки на 25%"),
                PerkType.Vampirism => ("Вампиризм", "Восстанавливает 10% от нанесённого урона"),
                PerkType.ExperienceBoost => ("Ускорение опыта", "Получайте на 50% больше опыта"),
                PerkType.AbilityFireBurst => ("Огненный взрыв", "Выпускает огненный взрыв с уроном 25 по площади (клавиша E)"),
                PerkType.AbilityBarrier => ("Барьер", "Становитесь неуязвимым на 2.5с и отталкивает врагов (клавиша E)"),
                PerkType.FireBurstRadiusUp => ("Огненный взрыв: Радиус+", "Увеличивает радиус взрыва на 30"),
                PerkType.FireBurstDamageUp => ("Огненный взрыв: Урон+", "Увеличивает урон взрыва на 15"),
                PerkType.FireBurstCooldownDown => ("Огненный взрыв: Быстрее", "Уменьшает перезарядку взрыва на 20%"),
                PerkType.BarrierDurationUp => ("Барьер: Дольше", "Увеличивает длительность барьера на 1с"),
                PerkType.BarrierCooldownDown => ("Барьер: Быстрее", "Уменьшает перезарядку барьера на 20%"),
                _ => ("", "")
            };
        }
    }
}
