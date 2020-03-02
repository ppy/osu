// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public abstract class ModSandboxTestScene : PlayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ModSandboxTestScene)
        };

        protected ModSandboxTestScene(Ruleset ruleset)
            : base(ruleset)
        {
        }

        private ModTestCaseData currentTest;

        public override void SetUpSteps()
        {
            foreach (var testCase in CreateTestCases())
            {
                AddStep("set test case", () => currentTest = testCase);
                base.SetUpSteps();
            }
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => currentTest.Beatmap;

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Append(currentTest.Mod).ToArray();

            if (currentTest.Autoplay)
            {
                // We're simulating an auto-play via a replay so that the auto-play mod does not interfere
                var beatmap = Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo, SelectedMods.Value);
                var score = ruleset.GetAutoplayMod().CreateReplayScore(beatmap);

                return new TestReplayPlayer(score, false, false);
            }

            return base.CreatePlayer(ruleset);
        }

        protected abstract ModTestCaseData[] CreateTestCases();

        protected class ModTestCaseData
        {
            public Mod Mod;
            public bool Autoplay;
            public IBeatmap Beatmap;
        }
    }
}
