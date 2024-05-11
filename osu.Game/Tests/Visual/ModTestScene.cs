// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;

namespace osu.Game.Tests.Visual
{
    public abstract partial class ModTestScene : PlayerTestScene
    {
        protected sealed override bool HasCustomSteps => true;

        protected ModTestData CurrentTestData { get; private set; }

        protected void CreateModTest(ModTestData testData) => CreateTest(() =>
        {
            AddStep("set test data", () => CurrentTestData = testData);
        });

        public override void TearDownSteps()
        {
            AddUntilStep("test passed", () =>
            {
                if (CurrentTestData == null)
                    return true;

                return CurrentTestData.PassCondition?.Invoke() ?? false;
            });

            base.TearDownSteps();
        }

        protected sealed override IBeatmap CreateBeatmap(RulesetInfo ruleset) => CurrentTestData?.Beatmap ?? base.CreateBeatmap(ruleset);

        protected sealed override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            var mods = new List<Mod>(SelectedMods.Value);

            if (CurrentTestData.Mods != null)
                mods.AddRange(CurrentTestData.Mods);
            if (CurrentTestData.Autoplay)
                mods.Add(ruleset.GetAutoplayMod());

            SelectedMods.Value = mods;

            return CreateModPlayer(ruleset);
        }

        protected virtual TestPlayer CreateModPlayer(Ruleset ruleset) => new ModTestPlayer(CurrentTestData, AllowFail);

        protected partial class ModTestPlayer : TestPlayer
        {
            private readonly bool allowFail;
            private readonly ModTestData currentTestData;

            protected override bool CheckModsAllowFailure() => allowFail;

            public ModTestPlayer(ModTestData data, bool allowFail)
                : base(true, false)
            {
                this.allowFail = allowFail;
                currentTestData = data;
            }

            protected override void PrepareReplay()
            {
                if (currentTestData.Autoplay && currentTestData.ReplayFrames?.Count > 0)
                    throw new InvalidOperationException(@$"{nameof(ModTestData.Autoplay)} must be false when {nameof(ModTestData.ReplayFrames)} is specified.");

                if (currentTestData.ReplayFrames != null)
                {
                    DrawableRuleset?.SetReplayScore(new Score
                    {
                        Replay = new Replay { Frames = currentTestData.ReplayFrames },
                        ScoreInfo = new ScoreInfo { User = new APIUser { Username = @"Test" } },
                    });
                }

                base.PrepareReplay();
            }
        }

        protected class ModTestData
        {
            /// <summary>
            /// Whether to use a replay to simulate an auto-play. True by default.
            /// </summary>
            public bool Autoplay = true;

            /// <summary>
            /// The frames to use for replay. <see cref="Autoplay"/> must be set to false.
            /// </summary>
            [CanBeNull]
            public List<ReplayFrame> ReplayFrames;

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
            /// The <see cref="Mod"/>s this test case tests.
            /// </summary>
            public IReadOnlyList<Mod> Mods;

            /// <summary>
            /// Convenience property for setting <see cref="Mods"/> if only
            /// a single mod is to be tested.
            /// </summary>
            public Mod Mod
            {
                set => Mods = new[] { value };
            }
        }
    }
}
