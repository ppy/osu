// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.Formats
{
    public class LegacyScoreEncoderTest
    {
        [TestCase(1, 3)]
        [TestCase(1, 0)]
        [TestCase(0, 3)]
        public void CatchMergesFruitAndDropletMisses(int missCount, int largeTickMissCount)
        {
            var ruleset = new CatchRuleset().RulesetInfo;
            var scoreInfo = TestResources.CreateTestScoreInfo(ruleset);
            var beatmap = new TestBeatmap(ruleset);

            scoreInfo.Statistics = new Dictionary<HitResult, int>
            {
                [HitResult.Great] = 50,
                [HitResult.LargeTickHit] = 5,
                [HitResult.Miss] = missCount,
                [HitResult.LargeTickMiss] = largeTickMissCount
            };

            var score = new Score { ScoreInfo = scoreInfo };
            var decodedAfterEncode = encodeThenDecode(LegacyBeatmapDecoder.LATEST_VERSION, score, beatmap, out _);

            Assert.That(decodedAfterEncode.ScoreInfo.GetCountMiss(), Is.EqualTo(missCount + largeTickMissCount));
        }

        [Test]
        public void ScoreWithMissIsNotPerfect()
        {
            var ruleset = new OsuRuleset().RulesetInfo;
            var scoreInfo = TestResources.CreateTestScoreInfo(ruleset);
            var beatmap = new TestBeatmap(ruleset);

            scoreInfo.Statistics = new Dictionary<HitResult, int>
            {
                [HitResult.Great] = 2,
                [HitResult.Miss] = 1,
            };

            scoreInfo.MaximumStatistics = new Dictionary<HitResult, int>
            {
                [HitResult.Great] = 3
            };

            // Hit -> Miss -> Hit
            scoreInfo.Combo = 1;
            scoreInfo.MaxCombo = 1;

            encodeThenDecode(LegacyBeatmapDecoder.LATEST_VERSION, new Score { ScoreInfo = scoreInfo }, beatmap, out var decoder);

            Assert.That(decoder.DecodedPerfectValue, Is.False);
        }

        private static Score encodeThenDecode(int beatmapVersion, Score score, TestBeatmap beatmap, out LegacyScoreDecoderTest.TestLegacyScoreDecoder decoder)
        {
            var encodeStream = new MemoryStream();

            var encoder = new LegacyScoreEncoder(score, beatmap);
            encoder.Encode(encodeStream);

            var decodeStream = new MemoryStream(encodeStream.GetBuffer());

            decoder = new LegacyScoreDecoderTest.TestLegacyScoreDecoder(beatmapVersion);
            var decodedAfterEncode = decoder.Parse(decodeStream);
            return decodedAfterEncode;
        }
    }
}
