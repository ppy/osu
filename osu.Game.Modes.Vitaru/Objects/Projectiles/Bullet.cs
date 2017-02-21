using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Vitaru.Objects;
using osu.Game.Modes.Vitaru.Objects.Characters;
using osu.Game.Modes.Vitaru.Objects.Drawables;
using osu.Framework.Graphics;
using OpenTK;
using System;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;
using OpenTK.Graphics;

namespace osu.Game.Modes.Vitaru.Objects.Projectiles
{
    public class Bullet : Projectile
    {
        public int bulletDamage { get; set; } = 20;
        public float bulletSpeed { get; set; } = 1;
        public Color4 bulletColor { get; set; } = Color4.Blue;
        public float bulletWidth { get; set; } = 12;
        public float bulletAngle { get; set; } = 0;
        public Vector2 bulletVelocity;

        public static int bulletsLoaded;
        public static int bulletCapHit;

        private DrawableBullet bulletSprite;


        public Bullet(int team)
        {
            bulletsLoaded++;
            Team = team;
            Children = new[]
            {
                bulletSprite = new DrawableBullet(this)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        protected override void Update()
        {
            base.Update();
            getBulletVelocity();
            MoveToOffset(new Vector2(bulletVelocity.X * (float)Clock.ElapsedFrameTime, bulletVelocity.Y * (float)Clock.ElapsedFrameTime));
            if (Position.Y < -375 | Position.X < -225 | Position.Y > 375 | Position.X > 225)
            {
                deleteBullet();
            }

            if (Clock.ElapsedFrameTime > 50)
            {
                bulletCapHit++;
                deleteBullet();
            }
        }
        public Vector2 getBulletVelocity()
        {
            bulletVelocity.Y = bulletSpeed * (-1 * ((float)Math.Cos(bulletAngle * (Math.PI / 180))));
            bulletVelocity.X = bulletSpeed * ((float)Math.Sin(bulletAngle * (Math.PI / 180)));
            return bulletVelocity;
        }
        internal void deleteBullet()
        {
            bulletsLoaded--;
            Dispose();
        }
    }
}