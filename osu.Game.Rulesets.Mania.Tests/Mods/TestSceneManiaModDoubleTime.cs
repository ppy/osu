// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModDoubleTime : ModTestScene
    {
        private const double offset = 18;

        protected override bool AllowFail => true;

        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestHitWindowWithoutDoubleTime() => CreateModTest(new ModTestData
        {
            PassCondition = () => Player.ScoreProcessor.JudgedHits > 0
                                  && Player.ScoreProcessor.Accuracy.Value == 1
                                  && Player.ScoreProcessor.TotalScore.Value == 1_000_000,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                BeatmapInfo = { Ruleset = new ManiaRuleset().RulesetInfo },
                Difficulty = { OverallDifficulty = 10 },
                HitObjects = new List<HitObject>
                {
                    new Note { StartTime = 1000 }
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new ManiaReplayFrame(1000 + offset, ManiaAction.Key1)
            }
        });

        [Test]
        public void TestHitWindowWithDoubleTime()
        {
            var doubleTime = new ManiaModDoubleTime();

            CreateModTest(new ModTestData
            {
                Mod = doubleTime,
                PassCondition = () => Player.ScoreProcessor.JudgedHits > 0
                                      && Player.ScoreProcessor.Accuracy.Value == 1
                                      && Player.ScoreProcessor.TotalScore.Value == (long)(1_000_010 * doubleTime.ScoreMultiplier),
                Autoplay = false,
                Beatmap = new Beatmap
                {
                    BeatmapInfo = { Ruleset = new ManiaRuleset().RulesetInfo },
                    Difficulty = { OverallDifficulty = 10 },
                    HitObjects = new List<HitObject>
                    {
                        new Note { StartTime = 1000 }
                    },
                },
                ReplayFrames = new List<ReplayFrame>
                {
                    new ManiaReplayFrame(1000 + offset, ManiaAction.Key1)
                }
            });
        }
    }
}
