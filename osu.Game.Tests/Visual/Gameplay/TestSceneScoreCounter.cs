// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneScoreCounter : OsuTestScene
    {
        public TestSceneScoreCounter()
        {
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

            AddStep(@"Reset all", delegate
            {
                score.Current.Value = 0;
                comboCounter.Current.Value = 0;
                numerator = denominator = 0;
                accuracyCounter.SetFraction(0, 0);
            });

            AddStep(@"Hit! :D", delegate
            {
                score.Current.Value += 300 + (ulong)(300.0 * (comboCounter.Current.Value > 0 ? comboCounter.Current.Value - 1 : 0) / 25.0);
                comboCounter.Increment();
                numerator++;
                denominator++;
                accuracyCounter.SetFraction(numerator, denominator);
            });

            AddStep(@"miss...", delegate
            {
                comboCounter.Current.Value = 0;
                denominator++;
                accuracyCounter.SetFraction(numerator, denominator);
            });
        }
    }
}
