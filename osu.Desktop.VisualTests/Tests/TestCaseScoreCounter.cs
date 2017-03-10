// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.UI;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseScoreCounter : TestCase
    {
        public override string Description => @"Tests multiple counters";

        public override void Reset()
        {
            base.Reset();

            int numerator = 0, denominator = 0;

            bool maniaHold = false;

            ScoreCounter score = new ScoreCounter(7)
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                TextSize = 40,
                Count = 0,
                Margin = new MarginPadding(20),
            };
            Add(score);

            ComboCounter standardCombo = new OsuComboCounter
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Margin = new MarginPadding(10),
                Count = 0,
                TextSize = 40,
            };
            Add(standardCombo);

            PercentageCounter accuracyCombo = new PercentageCounter
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                Position = new Vector2(-20, 60),
            };
            Add(accuracyCombo);

            StarCounter stars = new StarCounter
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(20, -160),
                Count = 5,
            };
            Add(stars);

            SpriteText starsLabel = new SpriteText
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(20, -190),
                Text = stars.Count.ToString("0.00"),
            };
            Add(starsLabel);

            AddButton(@"Reset all", delegate
            {
                score.Count = 0;
                standardCombo.Count = 0;
                numerator = denominator = 0;
                accuracyCombo.SetFraction(0, 0);
                stars.Count = 0;
                starsLabel.Text = stars.Count.ToString("0.00");
            });

            AddButton(@"Hit! :D", delegate
            {
                score.Count += 300 + (ulong)(300.0 * (standardCombo.Count > 0 ? standardCombo.Count - 1 : 0) / 25.0);
                standardCombo.Count++;
                numerator++; denominator++;
                accuracyCombo.SetFraction(numerator, denominator);
            });

            AddButton(@"miss...", delegate
            {
                standardCombo.Roll();
                denominator++;
                accuracyCombo.SetFraction(numerator, denominator);
            });

            AddButton(@"Alter stars", delegate
            {
                stars.Count = RNG.NextSingle() * (stars.StarCount + 1);
                starsLabel.Text = stars.Count.ToString("0.00");
            });

            AddButton(@"Stop counters", delegate
            {
                score.StopRolling();
                standardCombo.StopRolling();
                accuracyCombo.StopRolling();
                stars.StopAnimation();
            });
        }
    }
}
