//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
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
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects;
using osu.Framework.Timing;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseVitaruGameplay : TestCase
    {
        public override string Name => @"Vitaru Gameplay";
        public override string Description => @"Showing everything to play osu!vitaru";

        private VitaruPlayer player;
        private Enemy enemy;
        public int kills;
        public int combo;
        private SpriteText score;
        private SpriteText combox;

        //Score will probably be changed to reward points based on enemy difficulty
        private int perfect = 30;
        private int good = 20;
        private int bad = 10;
        private int graze = 5;

        public override void Reset()
        {
            base.Reset();
            kills = 0;
            combo = 0;
            Enemy.shoot = true;
            //ensure we are at offset 0
            //Clock = new FramedClock();

            player = new VitaruPlayer(this)
            {
                Anchor = Anchor.Centre,
                Shooting = true,
                OnDeath = NewPlayer
            };
            Add(player);

            enemy = new Enemy(this)
            {
                Anchor = Anchor.TopCentre,
                enemyPosition = new Vector2(0, 100),
                OnDeath = NewEnemy,
            };
            Add(enemy);

            score = new SpriteText()
            {
                Text = "" + (combo * (kills * perfect)),
                TextSize = 50,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            };
            Add(score);

            combox = new SpriteText()
            {
                Text = combo + "x",
                TextSize = 40,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft
            };
            Add(combox);
        }
        protected override void Update()
        {
            base.Update();
            score.Text = "" + (combo * (kills * perfect));
            combox.Text = combo + "x";
        }

        protected void NewEnemy()
        {
            kills++;
            combo++;
            enemy = new Enemy(this)
            {
                Anchor = Anchor.TopCentre,
                enemyPosition = new Vector2(new Random().Next(-200, 200), new Random().Next (50 , 200)),
                OnDeath = NewEnemy,
            };
            Add(enemy);
        }
        protected void NewPlayer()
        {
            combo = 0;
            player = new VitaruPlayer(this)
            {
                Anchor = Anchor.Centre,
                OnDeath = NewPlayer,
                Shooting = true,
            };
            Add(player);
        }
    }
}