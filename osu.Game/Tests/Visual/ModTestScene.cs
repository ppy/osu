// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

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

        private ModTestData currentTestData;

        protected void CreateModTest(ModTestData testData) => CreateTest(() =>
        {
            AddStep("set test data", () => currentTestData = testData);
        });

        public override void TearDownSteps()
        {
            AddUntilStep("test passed", () =>
            {
                if (currentTestData == null)
                    return true;

                return currentTestData.PassCondition?.Invoke() ?? false;
            });

            base.TearDownSteps();
        }

        protected sealed override IBeatmap CreateBeatmap(RulesetInfo ruleset) => currentTestData?.Beatmap ?? base.CreateBeatmap(ruleset);

        protected sealed override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            var mods = new List<Mod>(SelectedMods.Value);

            if (currentTestData.Mod != null)
                mods.Add(currentTestData.Mod);
            if (currentTestData.Autoplay)
                mods.Add(ruleset.GetAutoplayMod());

            SelectedMods.Value = mods;

            return CreateModPlayer(ruleset);
        }

        protected virtual TestPlayer CreateModPlayer(Ruleset ruleset) => new ModTestPlayer(AllowFail);

        protected class ModTestPlayer : TestPlayer
        {
            protected override bool AllowFail { get; }

            public ModTestPlayer(bool allowFail)
                : base(false, false)
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
            public Mod Mod;
        }
    }
}
