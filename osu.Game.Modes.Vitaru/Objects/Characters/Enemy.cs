using osu.Framework.Graphics.Containers;
using OpenTK;
using OpenTK.Input;
using osu.Game.Modes.Vitaru.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using System.Collections.Generic;
using System;
using OpenTK.Graphics;
using osu.Game.Modes.Vitaru.Objects.Projectiles;

namespace osu.Game.Modes.Vitaru.Objects.Characters
{
    public class Enemy : Character
    {
        //Combo Color will collor enemy clothes + Bullet center, Bullet outline should be opposite color
        public bool NewCombo { get; set; }
        public int StartTime { get; set; }
        private DrawableEnemy enemy;
        public static bool shoot = false;
        int a = 0;
        public Vector2 enemyPosition = new Vector2(0, -160);
        public Vector2 enemySpeed { get; set; } = new Vector2(0.5f, 0.5f);
        
        public Enemy(Container parent) : base(parent)
        {
            Children = new[]
            {
                enemy = new DrawableEnemy()
                {
                    Origin = Anchor.Centre,
                },
            };
            Health = 100;
            Team = 1;
            Add(hitbox = new Hitbox()
            {
                Alpha = 1,
                hitboxWidth = 20,
                hitboxColor = Color4.Cyan,
            });
        }
        protected override void Update()
        {
            base.Update();
            if (shoot == true)
            {
                enemyShoot();
            }

            float ySpeed = enemySpeed.Y * (float)(Clock.ElapsedFrameTime);
            float xSpeed = enemySpeed.X * (float)(Clock.ElapsedFrameTime);
            Position = enemyPosition;
        }
        private void enemyShoot()
        {
            a = (a + 31);
            Bullet b;
            parent.Add(b = new Bullet(Team)
            {
                Depth = 1,
                Anchor = Anchor.Centre,
                bulletAngle = a,
                bulletSpeed = 0.2f,
            });
            b.MoveTo(ToSpaceOfOtherDrawable(new Vector2(0, 0), b));
        }
    }
}