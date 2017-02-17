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
        public float bulletSpeed { get; set; }
        public Color4 bulletColor { get; internal set; } = Color4.Red;
        public float bulletWidth { get; set; } = 16;
        public float bulletAngle { get; set; }
        public Vector2 bulletVelocity;

        public static int bulletsLoaded = 0;
        public static int bulletCapHit = 0;

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

            if (Clock.ElapsedFrameTime > 40)
            {
                bulletCapHit++;
                deleteBullet();
            }
        }
        public Vector2 getBulletVelocity()
        {
            bulletVelocity.Y = bulletSpeed * (-1 * ((float)Math.Cos(bulletAngle * (3.1415f / 180))));
            bulletVelocity.X = bulletSpeed * ((float)Math.Sin(bulletAngle * (3.1415f / 180)));
            return bulletVelocity;
        }

        internal float getBulletRadius()
        {
            return (bulletWidth * 0.5f);
        }

        internal void deleteBullet()
        {
            bulletsLoaded--;
            Dispose();
        }
    }
}