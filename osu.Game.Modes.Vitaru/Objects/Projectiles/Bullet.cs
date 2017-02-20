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
        public float bulletAngle { get; set; } = 0;
        public Vector2 bulletVelocity;

        public static int bulletsLoaded;
        public static int bulletCapHit;

        private DrawableCircle bulletSprite;

        private float bulletWidth = 12;
        private Color4 bulletColor = Color4.Blue;

        public Color4 BulletColor
        {
            get
            {
                return bulletColor;
            }
            set
            {
                bulletColor = value;
                bulletSprite.CircleColor = value;
            }
        }
        public float BulletWidth
        {
            get
            {
                return bulletWidth;
            }
            set
            {
                bulletWidth = value;
                bulletSprite.CircleWidth = value;
            }
        }

        public Bullet(int team)
        {
            bulletsLoaded++;
            Team = team;
            Children = new[]
            {
                bulletSprite = new DrawableCircle
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
                Dispose();
            }

            if (Clock.ElapsedFrameTime > 50)
            {
                bulletCapHit++;
                Dispose();
            }
        }

        public Vector2 getBulletVelocity()
        {
            bulletVelocity.Y = bulletSpeed * (-1 * ((float)Math.Cos(bulletAngle * (Math.PI / 180))));
            bulletVelocity.X = bulletSpeed * ((float)Math.Sin(bulletAngle * (Math.PI / 180)));
            return bulletVelocity;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            bulletsLoaded--;
        }
    }
}