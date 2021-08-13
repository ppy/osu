// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableComboCounter : SkinnableHUDComponentTestScene
    {
        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor();

        protected override Drawable CreateDefaultImplementation() => new DefaultComboCounter();
        protected override Drawable CreateLegacyImplementation() => new LegacyComboCounter();

        [Test]
        public void TestComboCounterIncrementing()
        {
            AddRepeatStep("increase combo", () => scoreProcessor.Combo.Value++, 10);

            AddStep("reset combo", () => scoreProcessor.Combo.Value = 0);
        }
    }
}
