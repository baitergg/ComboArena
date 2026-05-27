using Microsoft.Xna.Framework;
namespace ComboArena.Model
{
    public class YellowEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Yellow;
        public YellowEnemy(float x, float y)
            : base(x, y,
                width: 100,
                height: 80,
                maxHealth: 60,
                speed: 60,
                damage: 10,
                attackCooldown: 1.8f,
                detectionRange: 500,
                experienceReward: 35)
        {
        }
    }
}
