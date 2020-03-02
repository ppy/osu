// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
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
                AddStep(testCase.Name, () => currentTest = testCase);
                base.SetUpSteps();
                AddUntilStep("test passed", () => testCase.PassCondition?.Invoke() ?? true);
            }
        }

        protected sealed override IBeatmap CreateBeatmap(RulesetInfo ruleset) => currentTest?.Beatmap ?? base.CreateBeatmap(ruleset);

        protected sealed override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Append(currentTest.Mod).ToArray();

            var score = currentTest.Autoplay
                ? ruleset.GetAutoplayMod().CreateReplayScore(Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo, SelectedMods.Value))
                : new Score { Replay = new Replay() };

            return CreateReplayPlayer(score);
        }

        /// <summary>
        /// Creates the test cases for this test scene.
        /// </summary>
        protected abstract ModTestCaseData[] CreateTestCases();

        /// <summary>
        /// Creates the <see cref="TestPlayer"/> for a test case.
        /// </summary>
        /// <param name="score">The <see cref="Score"/>.</param>
        protected virtual TestPlayer CreateReplayPlayer(Score score) => new TestPlayer(score);

        protected class TestPlayer : TestReplayPlayer
        {
            public TestPlayer(Score score)
                : base(score, false, false)
            {
            }
        }

        protected class ModTestCaseData
        {
            /// <summary>
            /// Whether to use a replay to simulate an auto-play. True by default.
            /// </summary>
            public bool Autoplay = true;

            /// <summary>
            /// The beatmap for this test case.
            /// </summary>
            [CanBeNull]
            public IBeatmap Beatmap;

            /// <summary>
            /// The conditions that cause this test case to pass.
            /// </summary>
            [CanBeNull]
            public Func<bool> PassCondition;

            /// <summary>
            /// The name of this test case, displayed in the test browser.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// The <see cref="Mod"/> this test case tests.
            /// </summary>
            public readonly Mod Mod;

            public ModTestCaseData(string name, Mod mod)
            {
                Name = name;
                Mod = mod;
            }
        }
    }
}
