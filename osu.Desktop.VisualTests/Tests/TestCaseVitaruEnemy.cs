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
    class TestCaseVitaruEnemy : TestCase
    {

        //private WorkingBeatmap beatmap;
        //private List<HitObject> enemysLoaded;

        public override string Name => @"Vitaru Enemy";
        public override string Description => @"Showing Enemy stuff";

        private VitaruPlayer player;
        private Enemy enemy;
        public int kills;
        public int combo;
        private SpriteText score;
        private SpriteText combox;
        private int perfect = 30;
        private int good = 20;
        private int bad = 10;
        private int graze = 5;

        public override void Reset()
        {
            base.Reset();
            kills = 0;

            player = new VitaruPlayer(this)
            {
                Anchor = Anchor.Centre,
                Shooting = true,
            };
            Add(player);

            AddButton(@"New Enemy", NewEnemy);

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
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            };
            Add(score);

            combox = new SpriteText()
            {
                Text = combo + "X",
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft
            };
            Add(combox);
        }
        protected override void Update()
        {
            base.Update();
            score.Text = "" + (combo * (kills * perfect));
            combox.Text = combo + "X";
        }

        protected void NewEnemy()
        {
            kills++;
            combo++;
            enemy = new Enemy(this)
            {
                Anchor = Anchor.TopCentre,
                enemyPosition = new Vector2(new Random().Next(-200, 200), new Random() .Next(50 , 200)),
                OnDeath = NewEnemy,
            };
            Add(enemy);
        }
    }
}