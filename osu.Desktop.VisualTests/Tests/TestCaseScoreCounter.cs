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

            ScoreCounter score = new ScoreCounter(7)
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                TextSize = 40,
                RollingDuration = 1000,
                RollingEasing = EasingTypes.Out,
                Count = 0,
                Position = new Vector2(20, 20),
            };
            Add(score);

            StandardComboCounter standardCombo = new StandardComboCounter
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
            Add(standardCombo);

            CatchComboCounter catchCombo = new CatchComboCounter
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                IsRollingProportional = true,
                RollingDuration = 20,
                PopOutDuration = 100,
                Count = 0,
                TextSize = 40,
            };
            Add(catchCombo);

            AlternativeComboCounter alternativeCombo = new AlternativeComboCounter
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
            Add(alternativeCombo);


            AccuracyCounter accuracyCombo = new AccuracyCounter
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                RollingDuration = 500,
                RollingEasing = EasingTypes.Out,
                Count = 100.0f,
                Position = new Vector2(20, 60),
            };
            Add(accuracyCombo);

            SpriteText starsLabel = new SpriteText
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(20, 190),
                Text = @"- unset -",
            };
            Add(starsLabel);

            StarCounter stars = new StarCounter
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(20, 160),
            };
            Add(stars);

            AddButton(@"Reset all", delegate
            {
                score.Count = 0;
                standardCombo.Count = 0;
                alternativeCombo.Count = 0;
                catchCombo.Count = 0;
                accuracyCombo.SetCount(0, 0);
                stars.Count = 0;
                starsLabel.Text = stars.Count.ToString("0.00");
            });

            AddButton(@"Hit! :D", delegate
            {
                score.Count += 300 + (ulong)(300.0 * (standardCombo.Count > 0 ? standardCombo.Count - 1 : 0) / 25.0);
                standardCombo.Count++;
                alternativeCombo.Count++;
                catchCombo.CatchFruit(new Color4(
                    Math.Max(0.5f, RNG.NextSingle()),
                    Math.Max(0.5f, RNG.NextSingle()),
                    Math.Max(0.5f, RNG.NextSingle()),
                    1)
                );
                accuracyCombo.Numerator++;
                accuracyCombo.Denominator++;
            });

            AddButton(@"miss...", delegate
            {
                standardCombo.RollBack();
                alternativeCombo.RollBack();
                catchCombo.RollBack();
                accuracyCombo.Denominator++;
            });

            AddButton(@"Alter stars", delegate
            {
                stars.Count = RNG.NextSingle() * (stars.MaxStars + 1);
                starsLabel.Text = stars.Count.ToString("0.00");
            });

            AddButton(@"Stop counters", delegate
            {
                score.StopRolling();
                standardCombo.StopRolling();
                catchCombo.StopRolling();
                alternativeCombo.StopRolling();
                accuracyCombo.StopRolling();
                stars.StopAnimation();
            });
        }
    }
}
