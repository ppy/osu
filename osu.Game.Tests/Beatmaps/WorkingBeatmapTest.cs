// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    public class WorkingBeatmapTest
    {
        [Test]
        public void TestModsApplicableToDifficultyReadFromDifficulty()
        {
            var beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo =
                {
                    BaseDifficulty =
                    {
                        OverallDifficulty = 11,
                        ApproachRate = 11,
                        DrainRate = 11,
                        CircleSize = 11
                    }
                }
            };

            var workingBeatmap = new TestWorkingBeatmap(beatmap);

            var playableBeatmap = workingBeatmap.GetPlayableBeatmap(new OsuRuleset().RulesetInfo, new[]
            {
                new OsuModDifficultyAdjust()
            });

            // Playable beatmap should have read default difficulty settings from beatmap.
            Assert.That(playableBeatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, Is.EqualTo(11));
            Assert.That(playableBeatmap.BeatmapInfo.BaseDifficulty.ApproachRate, Is.EqualTo(11));
            Assert.That(playableBeatmap.BeatmapInfo.BaseDifficulty.DrainRate, Is.EqualTo(11));
            Assert.That(playableBeatmap.BeatmapInfo.BaseDifficulty.CircleSize, Is.EqualTo(11));
        }
    }
}
