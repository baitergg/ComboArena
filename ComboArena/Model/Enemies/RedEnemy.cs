using Microsoft.Xna.Framework;
namespace ComboArena.Model
{
    public class RedEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Red;
        public RedEnemy(float x, float y)
            : base(x, y,
                width: 100,
                height: 80,
                maxHealth: 20,
                speed: 120,
                damage: 3,
                attackCooldown: 0.8f,
                detectionRange: 300,
                experienceReward: 10)
        {
        }
    }
}
