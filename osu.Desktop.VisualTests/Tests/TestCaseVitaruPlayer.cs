//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK.Input;
using osu.Framework.Timing;
using osu.Game.Modes.Vitaru.Objects.Characters;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using osu.Game.Modes.Vitaru.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Game.Database;
using osu.Game.Beatmaps;
using osu.Game.Modes;
using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Beatmaps.Formats;
using osu.Game.Screens.Play;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Vitaru.Objects;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseVitaruPlayer : TestCase
    {

        //private WorkingBeatmap beatmap;
        //private List<HitObject> enemys;

        public override string Name => @"Vitaru Player";

        public override string Description => @"Showing Player";

        private SpriteText health;
        private VitaruPlayer player;
        //private Hitbox hitbox;

        public override void Reset()
        {
            base.Reset();

            //ensure we are at offset 0
            //Clock = new FramedClock();

            player = new VitaruPlayer(this)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
            Add(player);

            AddKeyCounters();
            AddToggle(@"Toggle Kiai", player.ToggleKiai);

            health = new SpriteText()
            {
                Text = "Health: " + player.Health,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft
            };
            Add(health);
        }

        protected override void Update()
        {
            base.Update();
            health.Text = "Health: " + player.Health;
        }

        //Just the Keycounters on the edge of the screen
        public void AddKeyCounters()
        {
            Add(new[]
            {
            new KeyCounterCollection
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    IsCounting = true,
                    Children = new KeyCounter[]
                    {
                        new KeyCounterKeyboard(@"Up", Key.Up),
                    },
                },
            new KeyCounterCollection
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    IsCounting = true,
                    Children = new KeyCounter[]
                    {
                        new KeyCounterKeyboard(@"Down", Key.Down),
                    },
                },
            new KeyCounterCollection
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    IsCounting = true,
                    Children = new KeyCounter[]
                    {
                        new KeyCounterKeyboard(@"Left", Key.Left),
                    },
                },
            new KeyCounterCollection
                {
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    IsCounting = true,
                    Children = new KeyCounter[]
                    {
                        new KeyCounterKeyboard(@"Right", Key.Right),
                    },
                },
            new KeyCounterCollection
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    IsCounting = true,
                    Children = new KeyCounter[]
                    {
                        new KeyCounterKeyboard(@"LS", Key.LShift),
                        new KeyCounterKeyboard(@"Z", Key.Z),
                        new KeyCounterKeyboard(@"X", Key.X),
                        new KeyCounterKeyboard(@"RS", Key.RShift),
                    },
                },
            });
        }
    }
}