﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.UI;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseScoreCounter : TestCase
    {
        public override string Description => @"Tests multiple counters";

        public override void Reset()
        {
            base.Reset();

            int numerator = 0, denominator = 0;

            ScoreCounter score = new ScoreCounter(7)
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                TextSize = 40,
                Margin = new MarginPadding(20),
            };
            Add(score);

            ComboCounter comboCounter = new StandardComboCounter
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                Margin = new MarginPadding(10),
                TextSize = 40,
            };
            Add(comboCounter);

            PercentageCounter accuracyCounter = new PercentageCounter
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                Position = new Vector2(-20, 60),
            };
            Add(accuracyCounter);

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

            AddStep(@"Reset all", delegate
            {
                score.Current.Value = 0;
                comboCounter.Current.Value = 0;
                numerator = denominator = 0;
                accuracyCounter.SetFraction(0, 0);
                stars.Count = 0;
                starsLabel.Text = stars.Count.ToString("0.00");
            });

            AddStep(@"Hit! :D", delegate
            {
                score.Current.Value += 300 + (ulong)(300.0 * (comboCounter.Current > 0 ? comboCounter.Current - 1 : 0) / 25.0);
                comboCounter.Increment();
                numerator++; denominator++;
                accuracyCounter.SetFraction(numerator, denominator);
            });

            AddStep(@"miss...", delegate
            {
                comboCounter.Current.Value = 0;
                denominator++;
                accuracyCounter.SetFraction(numerator, denominator);
            });

            AddStep(@"Alter stars", delegate
            {
                stars.Count = RNG.NextSingle() * (stars.StarCount + 1);
                starsLabel.Text = stars.Count.ToString("0.00");
            });

            AddStep(@"Stop counters", delegate
            {
                score.StopRolling();
                comboCounter.StopRolling();
                accuracyCounter.StopRolling();
                stars.StopAnimation();
            });
        }
    }
}
