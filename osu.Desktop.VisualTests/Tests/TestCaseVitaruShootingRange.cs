using osu.Framework.Screens.Testing;
using osu.Game.Modes.Vitaru.Objects.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.Vitaru.Objects;
using OpenTK;
using osu.Game.Modes.Vitaru.Objects.Projectiles;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseVitaruShootingRange : TestCase
    {

        public override string Name => @"Shooting Range";
        public override string Description => @"Player Shooting mechanics";

        private VitaruPlayer player;
        private SpriteText bulletsOnScreen;
        private Enemy enemy;
        private SpriteText bulletCapHit;

        public override void Reset()
        {
            base.Reset();
            Bullet.bulletsLoaded = 0;

            player = new VitaruPlayer(this)
            {
                Anchor = Anchor.Centre,
                Shooting = true,
                OnDeath = NewPlayer,
            };
            Add(player);

            enemy = new Enemy(this)
            {
                Anchor = Anchor.TopCentre,
                enemyPosition = new Vector2(0, 100),
                OnDeath = NewEnemy,
            };
            Add(enemy);

            bulletsOnScreen = new SpriteText()
            {
                Text = "Bullets On Screen: " + Bullet.bulletsLoaded,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft
            };
            Add(bulletsOnScreen);

            bulletCapHit = new SpriteText()
            {
                Text = "Bullets Deleted Due to Cap hit: " + Bullet.bulletCapHit,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            };
            Add(bulletCapHit);
        }
        protected override void Update()
        {
            base.Update();
            bulletsOnScreen.Text = "Bullets On Screen: " + Bullet.bulletsLoaded;
            bulletCapHit.Text = "Bullets Deleted Due to Cap hit: " + Bullet.bulletCapHit;
        }

        protected void NewEnemy()
        {
            enemy = new Enemy(this)
            {
                Anchor = Anchor.TopCentre,
                enemyPosition = new Vector2(new Random().Next(-200, 200), 100),
                OnDeath = NewEnemy,
            };
            Add(enemy);
        }
        protected void NewPlayer()
        {
            player = new VitaruPlayer(this)
            {
                Anchor = Anchor.Centre,
                OnDeath = NewPlayer,
            };
            Add(player);
        }
    }
}