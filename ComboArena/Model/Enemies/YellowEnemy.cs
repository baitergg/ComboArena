namespace ComboArena.Model
{
    /// <summary>
    /// Жёлтый враг - тяжёлый, медленный, но с большим
    /// запасом здоровья и высоким уроном.
    /// Базовая роль: поддержка на дистанции.
    /// Способность: Лазер - стреляет лучом, наносящим урон на расстоянии.
    /// </summary>
    public class YellowEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Yellow;
        
        public YellowEnemy(float x, float y)
            : base(x, y,
                width: 143,
                height: 80,
                maxHealth: 60,
                speed: 60,
                damage: 10,
                attackCooldown: 1.8f,
                detectionRange: 500,
                experienceReward: 35)
        {
        }

        protected override void UnlockAbilities()
        {
            switch (Tier)
            {
                case EnemyTier.Elite:
                    InitLaserAbility(damage: 15, range: 300f);
                    break;

                case EnemyTier.Champion:
                    InitLaserAbility(damage: 20, range: 400f);
                    break;

                case EnemyTier.Boss:
                    InitLaserAbility(damage: 25, range: 500f);
                    break;
            }
        }
    }
}
