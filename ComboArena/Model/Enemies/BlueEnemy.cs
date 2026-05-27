using Microsoft.Xna.Framework;
namespace ComboArena.Model
{
    public class BlueEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Blue;
        public BlueEnemy(float x, float y)
            : base(x, y,
                width: 100,
                height: 80,
                maxHealth: 40,
                speed: 90,
                damage: 6,
                attackCooldown: 1.2f,
                detectionRange: 400,
                experienceReward: 20)
        {
        }
    }
}
