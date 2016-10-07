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

namespace osu.Desktop.Tests
{
    class TestCaseScoreCounter : TestCase
    {
        public override string Name => @"ScoreCounter";

        public override string Description => @"Tests multiple counters";

        public override void Reset()
        {
            base.Reset();

            Random rnd = new Random();

            ScoreCounter uc = new ScoreCounter
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                TextSize = 40,
                RollingDuration = 1000,
                RollingEasing = EasingTypes.Out,
                Count = 0,
                Position = new Vector2(20, 20),
                LeadingZeroes = 7,
            };
            Add(uc);

            StandardComboCounter sc = new StandardComboCounter
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(20, 20),
                IsRollingProportional = true,
                RollingDuration = 20,
                PopOutDuration = 250,
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
                PopOutDuration = 250,
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

            Button resetButton = new Button
            {
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                Text = @"Reset all",
                Width = 100,
                Height = 20,
                Position = new Vector2(0, 0),
            };
            resetButton.Action += delegate
            {
                uc.Count = 0;
                sc.Count = 0;
                ac.Count = 0;
                cc.Count = 0;
                pc.SetCount(0, 0);
            };
            Add(resetButton);

            Button hitButton = new Button
            {
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                Text = @"Hit! :D",
                Width = 100,
                Height = 20,
                Position = new Vector2(0, 20),
            };
            hitButton.Action += delegate
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
            };
            Add(hitButton);

            Button missButton = new Button
            {
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                Text = @"miss...",
                Width = 100,
                Height = 20,
                Position = new Vector2(0, 40),
            };
            missButton.Action += delegate
            {
                sc.Count = 0;
                ac.Count = 0;
                cc.Count = 0;
                pc.Denominator++;
            };
            Add(missButton);

            Button forceResetButton = new Button
            {
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                Text = @"Force reset",
                Width = 100,
                Height = 20,
                Position = new Vector2(0, 60),
            };
            forceResetButton.Action += delegate
            {
                uc.ResetCount();
                sc.ResetCount();
                ac.ResetCount();
                pc.ResetCount();
                cc.ResetCount();
            };
            Add(forceResetButton);

            Button stopButton = new Button
            {
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                Text = @"STOP!",
                Width = 100,
                Height = 20,
                Position = new Vector2(0, 80),
            };
            stopButton.Action += delegate
            {
                uc.StopRolling();
                sc.StopRolling();
                cc.StopRolling();
                ac.StopRolling();
                pc.StopRolling();
            };
            Add(stopButton);
        }
    }
}
