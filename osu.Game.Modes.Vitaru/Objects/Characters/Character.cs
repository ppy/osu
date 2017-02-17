using osu.Framework.Graphics.Containers;
using System;
using osu.Framework.Graphics;
using osu.Game.Modes.Vitaru.Objects.Projectiles;
using OpenTK;

namespace osu.Game.Modes.Vitaru.Objects.Characters
{
    public abstract class Character : Container
    {
        public float Health { get; internal set; } = 100;
        public float Armor { get; internal set; } = 1; //All damage taken should be divided by this number. During kiai player will only take half damage so [2]
        public int Team { get; set; } = 0; // 0 = Player, 1 = Ememies + Boss(s) in Singleplayer
        public int ProjectileDamage { get; set; }
        public float HitboxWidth { get; set; } = 12;
        public float BulletWidth { get; set; } = 16;
        public int BPM { get; set; } = 180;

        protected Hitbox hitbox;
        protected Container parent;

        public bool Shooting { get; set; } = false;

        private double timeSinceLastShoot;

        public Action OnDeath { get; set; }
        public Action OnShoot { get; set; }

        public Character(Container parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// The <see cref="Character"/> gets damaged, with a multiplier of <see cref="DamageMultiplier"/>
        /// </summary>
        /// <param name="damage">Damage without the Resistance applied</param>
        /// <returns>If the Character died</returns>
        public bool TakeDamage(int damage)
        {
            Health -= (int)(damage * Armor);
            if (Health <= 0)
            {
                Dispose();
                OnDeath();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Heals the <see cref="Character"/> by the specified amount
        /// </summary>
        /// <param name="health">Amount of health to be healed</param>
        public void Heal(int health)
        {
            Health += health;
        }

        protected override void Update()
        {
            base.Update();
            foreach (Drawable draw in parent.Children)
            {
                if (draw is Bullet)
                {
                    Bullet bullet = draw as Bullet;
                    if (bullet.Team != Team)
                    {
                        Vector2 bulletPos = bullet.ToSpaceOfOtherDrawable(Vector2.Zero, this);
                        float distance = (float)Math.Sqrt(Math.Pow(bulletPos.X, 2) + Math.Pow(bulletPos.Y, 2));
                        float minDist = hitbox.GetRadius()+bullet.getBulletRadius();
                        if (distance < minDist)
                        {
                            bullet.deleteBullet();
                            if (TakeDamage(bullet.bulletDamage))
                                break;
                        }
                    }
                }
            }
            if (Shooting)
            {
                timeSinceLastShoot += Clock.ElapsedFrameTime;
                if ((timeSinceLastShoot / 1000.0) > 1 / (BPM / 30.0))
                {
                    if(OnShoot != null)
                        OnShoot();
                    timeSinceLastShoot -= 1 / (BPM / 30.0) * 1000.0;
                }
            }
        }

    }
}
