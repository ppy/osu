// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
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
        private CultureInfo originalCulture;

        [SetUp]
        public void SetUp()
        {
            originalCulture = CultureInfo.CurrentCulture;
        }

        [Test]
        public void TestDecodeManiaReplay()
        {
            var decoder = new TestLegacyScoreDecoder();

            using (var resourceStream = TestResources.OpenResource("Replays/mania-replay.osr"))
            {
                var score = decoder.Parse(resourceStream);

                Assert.AreEqual(3, score.ScoreInfo.Ruleset.OnlineID);

                Assert.AreEqual(2, score.ScoreInfo.Statistics[HitResult.Great]);
                Assert.AreEqual(1, score.ScoreInfo.Statistics[HitResult.Good]);

                Assert.AreEqual(829_931, score.ScoreInfo.TotalScore);
                Assert.AreEqual(3, score.ScoreInfo.MaxCombo);

                Assert.IsTrue(score.ScoreInfo.Mods.Any(m => m is ManiaModClassic));
                Assert.IsTrue(score.ScoreInfo.APIMods.Any(m => m.Acronym == "CL"));
                Assert.IsTrue(score.ScoreInfo.ModsJson.Contains("CL"));

                Assert.IsTrue(Precision.AlmostEquals(0.8889, score.ScoreInfo.Accuracy, 0.0001));
                Assert.AreEqual(ScoreRank.B, score.ScoreInfo.Rank);

                Assert.That(score.Replay.Frames, Is.Not.Empty);
            }
        }

        [Test]
        public void TestDecodeTaikoReplay()
        {
            var decoder = new TestLegacyScoreDecoder();

            using (var resourceStream = TestResources.OpenResource("Replays/taiko-replay.osr"))
            {
                var score = decoder.Parse(resourceStream);

                Assert.AreEqual(1, score.ScoreInfo.Ruleset.OnlineID);
                Assert.AreEqual(4, score.ScoreInfo.Statistics[HitResult.Great]);
                Assert.AreEqual(2, score.ScoreInfo.Statistics[HitResult.LargeBonus]);
                Assert.AreEqual(4, score.ScoreInfo.MaxCombo);

                Assert.That(score.Replay.Frames, Is.Not.Empty);
            }
        }

        [Test]
        public void TestDecodeLegacyOnlineID()
        {
            var decoder = new TestLegacyScoreDecoder();

            using (var resourceStream = TestResources.OpenResource("Replays/taiko-replay-with-legacy-online-id.osr"))
            {
                var score = decoder.Parse(resourceStream);

                Assert.That(score.ScoreInfo.OnlineID, Is.EqualTo(-1));
                Assert.That(score.ScoreInfo.LegacyOnlineID, Is.EqualTo(255));
            }
        }

        [Test]
        public void TestDecodeNewOnlineID()
        {
            var decoder = new TestLegacyScoreDecoder();

            using (var resourceStream = TestResources.OpenResource("Replays/taiko-replay-with-new-online-id.osr"))
            {
                var score = decoder.Parse(resourceStream);

                Assert.That(score.ScoreInfo.OnlineID, Is.EqualTo(258));
                Assert.That(score.ScoreInfo.LegacyOnlineID, Is.EqualTo(-1));
            }
        }

        [TestCase(3, true)]
        [TestCase(6, false)]
        [TestCase(LegacyBeatmapDecoder.LATEST_VERSION, false)]
        public void TestLegacyBeatmapReplayOffsetsDecode(int beatmapVersion, bool offsetApplied)
        {
            const double first_frame_time = 48;
            const double second_frame_time = 65;

            var decoder = new TestLegacyScoreDecoder(beatmapVersion);

            using (var resourceStream = TestResources.OpenResource("Replays/mania-replay.osr"))
            {
                var score = decoder.Parse(resourceStream);

                Assert.That(score.Replay.Frames[0].Time, Is.EqualTo(first_frame_time + (offsetApplied ? LegacyBeatmapDecoder.EARLY_VERSION_TIMING_OFFSET : 0)));
                Assert.That(score.Replay.Frames[1].Time, Is.EqualTo(second_frame_time + (offsetApplied ? LegacyBeatmapDecoder.EARLY_VERSION_TIMING_OFFSET : 0)));
            }
        }

        [TestCase(3)]
        [TestCase(6)]
        [TestCase(LegacyBeatmapDecoder.LATEST_VERSION)]
        public void TestLegacyBeatmapReplayOffsetsEncodeDecode(int beatmapVersion)
        {
            const double first_frame_time = 2000;
            const double second_frame_time = 3000;

            var ruleset = new OsuRuleset().RulesetInfo;
            var scoreInfo = TestResources.CreateTestScoreInfo(ruleset);
            var beatmap = new TestBeatmap(ruleset)
            {
                BeatmapInfo =
                {
                    BeatmapVersion = beatmapVersion
                }
            };

            var score = new Score
            {
                ScoreInfo = scoreInfo,
                Replay = new Replay
                {
                    Frames = new List<ReplayFrame>
                    {
                        new OsuReplayFrame(first_frame_time, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton),
                        new OsuReplayFrame(second_frame_time, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton)
                    }
                }
            };

            var decodedAfterEncode = encodeThenDecode(beatmapVersion, score, beatmap);

            Assert.That(decodedAfterEncode.Replay.Frames[0].Time, Is.EqualTo(first_frame_time));
            Assert.That(decodedAfterEncode.Replay.Frames[1].Time, Is.EqualTo(second_frame_time));
        }

        [Test]
        public void TestCultureInvariance()
        {
            var ruleset = new OsuRuleset().RulesetInfo;
            var scoreInfo = TestResources.CreateTestScoreInfo(ruleset);
            var beatmap = new TestBeatmap(ruleset);
            var score = new Score
            {
                ScoreInfo = scoreInfo,
                Replay = new Replay
                {
                    Frames = new List<ReplayFrame>
                    {
                        new OsuReplayFrame(2000, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton)
                    }
                }
            };

            // the "se" culture is used here, as it encodes the negative number sign as U+2212 MINUS SIGN,
            // rather than the classic ASCII U+002D HYPHEN-MINUS.
            CultureInfo.CurrentCulture = new CultureInfo("se");

            var decodedAfterEncode = encodeThenDecode(LegacyBeatmapDecoder.LATEST_VERSION, score, beatmap);

            Assert.Multiple(() =>
            {
                Assert.That(decodedAfterEncode, Is.Not.Null);

                Assert.That(decodedAfterEncode.ScoreInfo.User.Username, Is.EqualTo(scoreInfo.User.Username));
                Assert.That(decodedAfterEncode.ScoreInfo.Ruleset, Is.EqualTo(scoreInfo.Ruleset));
                Assert.That(decodedAfterEncode.ScoreInfo.TotalScore, Is.EqualTo(scoreInfo.TotalScore));
                Assert.That(decodedAfterEncode.ScoreInfo.MaxCombo, Is.EqualTo(scoreInfo.MaxCombo));
                Assert.That(decodedAfterEncode.ScoreInfo.Date, Is.EqualTo(scoreInfo.Date));

                Assert.That(decodedAfterEncode.Replay.Frames.Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestSoloScoreData()
        {
            var ruleset = new OsuRuleset().RulesetInfo;

            var scoreInfo = TestResources.CreateTestScoreInfo(ruleset);
            scoreInfo.Mods = new Mod[]
            {
                new OsuModDoubleTime { SpeedChange = { Value = 1.1 } }
            };

            var beatmap = new TestBeatmap(ruleset);
            var score = new Score
            {
                ScoreInfo = scoreInfo,
                Replay = new Replay
                {
                    Frames = new List<ReplayFrame>
                    {
                        new OsuReplayFrame(2000, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton)
                    }
                }
            };

            var decodedAfterEncode = encodeThenDecode(LegacyBeatmapDecoder.LATEST_VERSION, score, beatmap);

            Assert.Multiple(() =>
            {
                Assert.That(decodedAfterEncode.ScoreInfo.Statistics, Is.EqualTo(scoreInfo.Statistics));
                Assert.That(decodedAfterEncode.ScoreInfo.MaximumStatistics, Is.EqualTo(scoreInfo.MaximumStatistics));
                Assert.That(decodedAfterEncode.ScoreInfo.Mods, Is.EqualTo(scoreInfo.Mods));
            });
        }

        private static Score encodeThenDecode(int beatmapVersion, Score score, TestBeatmap beatmap)
        {
            var encodeStream = new MemoryStream();

            var encoder = new LegacyScoreEncoder(score, beatmap);
            encoder.Encode(encodeStream);

            var decodeStream = new MemoryStream(encodeStream.GetBuffer());

            var decoder = new TestLegacyScoreDecoder(beatmapVersion);
            var decodedAfterEncode = decoder.Parse(decodeStream);
            return decodedAfterEncode;
        }

        [TearDown]
        public void TearDown()
        {
            CultureInfo.CurrentCulture = originalCulture;
        }

        private class TestLegacyScoreDecoder : LegacyScoreDecoder
        {
            private readonly int beatmapVersion;

            private static readonly Dictionary<int, Ruleset> rulesets = new Ruleset[]
            {
                new OsuRuleset(),
                new TaikoRuleset(),
                new CatchRuleset(),
                new ManiaRuleset()
            }.ToDictionary(ruleset => ((ILegacyRuleset)ruleset).LegacyID);

            public TestLegacyScoreDecoder(int beatmapVersion = LegacyBeatmapDecoder.LATEST_VERSION)
            {
                this.beatmapVersion = beatmapVersion;
            }

            protected override Ruleset GetRuleset(int rulesetId) => rulesets[rulesetId];

            protected override WorkingBeatmap GetBeatmap(string md5Hash) => new TestWorkingBeatmap(new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    MD5Hash = md5Hash,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    Difficulty = new BeatmapDifficulty(),
                    BeatmapVersion = beatmapVersion,
                }
            });
        }
    }
}
