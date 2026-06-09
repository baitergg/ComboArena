namespace ComboArena.Model
{
    /// <summary>
    /// Красный враг - лёгкий, быстрый, но слабый.
    /// Базовая роль: мобильный юнит для постоянного давления.
    /// Способность: Ярость - при низком HP сильно ускоряется.
    /// </summary>
    public class RedEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Red;
        
        public RedEnemy(float x, float y)
            : base(x, y,
                width: 143,
                height: 80,
                maxHealth: 20,
                speed: 120,
                damage: 3,
                attackCooldown: 0.8f,
                detectionRange: 300,
                experienceReward: 10)
        {
        }

        protected override void UnlockAbilities()
        {
            switch (Tier)
            {
                case EnemyTier.Elite:
                    InitEnrageAbility(1.5f);
                    break;

                case EnemyTier.Champion:
                    InitEnrageAbility(2.0f);
                    break;

                case EnemyTier.Boss:
                    Speed = BaseSpeed * 1.3f;
                    InitEnrageAbility(2.5f);
                    break;
            }
        }
    }
}
