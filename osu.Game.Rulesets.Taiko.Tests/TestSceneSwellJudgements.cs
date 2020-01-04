// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestSceneSwellJudgements : PlayerTestScene
    {
        protected new TestPlayer Player => (TestPlayer)base.Player;

        public TestSceneSwellJudgements()
            : base(new TaikoRuleset())
        {
        }

        [Test]
        public void TestZeroTickTimeOffsets()
        {
            AddUntilStep("gameplay finished", () => Player.ScoreProcessor.HasCompleted);
            AddAssert("all tick offsets are 0", () => Player.Results.Where(r => r.Judgement is TaikoSwellTickJudgement).All(r => r.TimeOffset == 0));
        }

        protected override bool Autoplay => true;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap<TaikoHitObject>
            {
                BeatmapInfo = { Ruleset = new TaikoRuleset().RulesetInfo },
                HitObjects =
                {
                    new Swell
                    {
                        StartTime = 1000,
                        Duration = 1000,
                    }
                }
            };

            return beatmap;
        }

        protected override Player CreatePlayer(Ruleset ruleset) => new TestPlayer();

        protected class TestPlayer : Player
        {
            public readonly List<JudgementResult> Results = new List<JudgementResult>();

            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public TestPlayer()
                : base(false, false)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                ScoreProcessor.NewJudgement += r => Results.Add(r);
            }
        }
    }
}
