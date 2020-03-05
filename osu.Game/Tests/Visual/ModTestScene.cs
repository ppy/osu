// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public abstract class ModTestScene : PlayerTestScene
    {
        protected sealed override bool HasCustomSteps => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ModTestScene)
        };

        protected ModTestScene(Ruleset ruleset)
            : base(ruleset)
        {
        }

        private ModTestData currentTest;

        protected void CreateModTest(ModTestData testData) => CreateTest(() =>
        {
            AddStep("set test data", () => currentTest = testData);
        });

        public override void TearDownSteps()
        {
            AddUntilStep("test passed", () =>
            {
                if (currentTest == null)
                    return true;

                return currentTest.PassCondition?.Invoke() ?? false;
            });

            base.TearDownSteps();
        }

        protected sealed override IBeatmap CreateBeatmap(RulesetInfo ruleset) => currentTest?.Beatmap ?? base.CreateBeatmap(ruleset);

        protected sealed override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Append(currentTest.Mod).ToArray();

            var score = currentTest.Autoplay
                ? ruleset.GetAutoplayMod().CreateReplayScore(Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo, SelectedMods.Value))
                : null;

            return CreateReplayPlayer(score, AllowFail);
        }

        /// <summary>
        /// Creates the <see cref="TestPlayer"/> for a test case.
        /// </summary>
        /// <param name="score">The <see cref="Score"/>.</param>
        /// <param name="allowFail">Whether the player can fail.</param>
        protected virtual TestPlayer CreateReplayPlayer(Score score, bool allowFail) => new TestPlayer(score, allowFail);

        protected class TestPlayer : TestReplayPlayer
        {
            protected override bool AllowFail { get; }

            public TestPlayer(Score score, bool allowFail)
                : base(score, false, false)
            {
                AllowFail = allowFail;
            }
        }

        protected class ModTestData
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
            /// The <see cref="Mod"/> this test case tests.
            /// </summary>
            public readonly Mod Mod;

            public ModTestData(Mod mod)
            {
                Mod = mod;
            }
        }
    }
}
