// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableAccuracyCounter : SkinnableTestScene
    {
        private IEnumerable<SkinnableAccuracyCounter> accuracyCounters => CreatedDrawables.OfType<SkinnableAccuracyCounter>();

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create combo counters", () => SetContents(() => new SkinnableAccuracyCounter()));
        }

        [Test]
        public void TestChangingAccuracy()
        {
            AddStep(@"Reset all", () => scoreProcessor.Accuracy.Value = 1);

            AddStep(@"Hit! :D", () => scoreProcessor.Accuracy.Value -= 0.23);
        }
    }
}
