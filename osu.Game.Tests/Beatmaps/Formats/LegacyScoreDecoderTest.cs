// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyScoreDecoderTest
    {
        [Test]
        public void TestDecodeManiaReplay()
        {
            var decoder = new TestLegacyScoreDecoder();

            using (var resourceStream = TestResources.OpenResource("Replays/mania-replay.osr"))
            {
                var score = decoder.Parse(resourceStream);

                Assert.AreEqual(3, score.ScoreInfo.Ruleset.ID);

                Assert.AreEqual(2, score.ScoreInfo.Statistics[HitResult.Great]);
                Assert.AreEqual(1, score.ScoreInfo.Statistics[HitResult.Good]);

                Assert.AreEqual(829_931, score.ScoreInfo.TotalScore);
                Assert.AreEqual(3, score.ScoreInfo.MaxCombo);
                Assert.IsTrue(Precision.AlmostEquals(0.8889, score.ScoreInfo.Accuracy, 0.0001));
                Assert.AreEqual(ScoreRank.B, score.ScoreInfo.Rank);

                Assert.That(score.Replay.Frames, Is.Not.Empty);
            }
        }

        private class TestLegacyScoreDecoder : LegacyScoreDecoder
        {
            private static readonly Dictionary<int, Ruleset> rulesets = new Ruleset[]
            {
                new OsuRuleset(),
                new TaikoRuleset(),
                new CatchRuleset(),
                new ManiaRuleset()
            }.ToDictionary(ruleset => ((ILegacyRuleset)ruleset).LegacyID);

            protected override Ruleset GetRuleset(int rulesetId) => rulesets[rulesetId];

            protected override WorkingBeatmap GetBeatmap(string md5Hash) => new TestWorkingBeatmap(new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    MD5Hash = md5Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    BaseDifficulty = new BeatmapDifficulty()
                }
            });
        }
    }
}
