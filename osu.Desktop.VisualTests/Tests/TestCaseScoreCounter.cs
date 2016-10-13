//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Input;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Transformations;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Graphics.Sprites;

namespace osu.Desktop.Tests
{
    class TestCaseScoreCounter : TestCase
    {
        public override string Name => @"ScoreCounter";

        public override string Description => @"Tests multiple counters";

        public override void Reset()
        {
            base.Reset();

            ScoreCounter uc = new ScoreCounter(7)
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                TextSize = 40,
                RollingDuration = 1000,
                RollingEasing = EasingTypes.Out,
                Count = 0,
                Position = new Vector2(20, 20),
            };
            Add(uc);

            StandardComboCounter sc = new StandardComboCounter
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(10, 10),
                InnerCountPosition = new Vector2(10, 10),
                IsRollingProportional = true,
                RollingDuration = 20,
                PopOutDuration = 100,
                Count = 0,
                TextSize = 40,
            };
            Add(sc);

            CatchComboCounter cc = new CatchComboCounter
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                IsRollingProportional = true,
                RollingDuration = 20,
                PopOutDuration = 100,
                Count = 0,
                TextSize = 40,
            };
            Add(cc);

            AlternativeComboCounter ac = new AlternativeComboCounter
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(20, 80),
                IsRollingProportional = true,
                RollingDuration = 20,
                ScaleFactor = 2,
                Count = 0,
                TextSize = 40,
            };
            Add(ac);


            AccuracyCounter pc = new AccuracyCounter
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                RollingDuration = 1000,
                RollingEasing = EasingTypes.Out,
                Count = 100.0f,
                Position = new Vector2(20, 60),
            };
            Add(pc);

            SpriteText text = new SpriteText
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(20, 190),
                Text = @"- unset -",
            };
            Add(text);

            StarCounter tc = new StarCounter
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(20, 160),
            };
            Add(tc);

            AddButton(@"Reset all", delegate
            {
                uc.Count = 0;
                sc.Count = 0;
                ac.Count = 0;
                cc.Count = 0;
                pc.SetCount(0, 0);
                tc.Count = 0;
                text.Text = tc.Count.ToString("0.00");
            });

            AddButton(@"Hit! :D", delegate
            {
                uc.Count += 300 + (ulong)(300.0 * (sc.Count > 0 ? sc.Count - 1 : 0) / 25.0);
                sc.Count++;
                ac.Count++;
                cc.CatchFruit(new Color4(
                    Math.Max(0.5f, RNG.NextSingle()),
                    Math.Max(0.5f, RNG.NextSingle()),
                    Math.Max(0.5f, RNG.NextSingle()),
                    1)
                );
                pc.Numerator++;
                pc.Denominator++;
            });

            AddButton(@"miss...", delegate
            {
                sc.Count = 0;
                ac.Count = 0;
                cc.Count = 0;
                pc.Denominator++;
            });

            AddButton(@"Alter stars", delegate
            {
                tc.Count = RNG.NextSingle() * (tc.MaxStars + 1);
                text.Text = tc.Count.ToString("0.00");
            });

            AddButton(@"Stop counters", delegate
            {
                uc.StopRolling();
                sc.StopRolling();
                cc.StopRolling();
                ac.StopRolling();
                pc.StopRolling();
                tc.StopRolling();
            });
        }
    }
}
