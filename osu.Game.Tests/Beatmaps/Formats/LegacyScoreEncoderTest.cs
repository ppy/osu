// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO.Legacy;
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
            var decodedAfterEncode = encodeThenDecode(LegacyBeatmapDecoder.LATEST_VERSION, score, beatmap);

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

            using (var ms = new MemoryStream())
            {
                new LegacyScoreEncoder(new Score { ScoreInfo = scoreInfo }, beatmap).Encode(ms, true);

                ms.Seek(0, SeekOrigin.Begin);

                using (var sr = new SerializationReader(ms))
                {
                    sr.ReadByte(); // ruleset id
                    sr.ReadInt32(); // version
                    sr.ReadString(); // beatmap hash
                    sr.ReadString(); // username
                    sr.ReadString(); // score hash
                    sr.ReadInt16(); // count300
                    sr.ReadInt16(); // count100
                    sr.ReadInt16(); // count50
                    sr.ReadInt16(); // countGeki
                    sr.ReadInt16(); // countKatu
                    sr.ReadInt16(); // countMiss
                    sr.ReadInt32(); // total score
                    sr.ReadInt16(); // max combo
                    bool isPerfect = sr.ReadBoolean(); // full combo

                    Assert.That(isPerfect, Is.False);
                }
            }
        }

        private static Score encodeThenDecode(int beatmapVersion, Score score, TestBeatmap beatmap)
        {
            var encodeStream = new MemoryStream();

            var encoder = new LegacyScoreEncoder(score, beatmap);
            encoder.Encode(encodeStream);

            var decodeStream = new MemoryStream(encodeStream.GetBuffer());

            var decoder = new LegacyScoreDecoderTest.TestLegacyScoreDecoder(beatmapVersion);
            var decodedAfterEncode = decoder.Parse(decodeStream);
            return decodedAfterEncode;
        }
    }
}
