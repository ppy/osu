// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Legacy;
using osu.Game.IO.Legacy;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK;

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

                Assert.AreEqual(829_931, score.ScoreInfo.LegacyTotalScore);
                Assert.AreEqual(3, score.ScoreInfo.MaxCombo);

                Assert.That(score.ScoreInfo.APIMods.Select(m => m.Acronym), Is.EquivalentTo(new[] { "CL", "9K", "DS" }));

                Assert.That((2 * 300d + 1 * 200) / (3 * 305d), Is.EqualTo(score.ScoreInfo.Accuracy).Within(0.0001));
                Assert.AreEqual(ScoreRank.B, score.ScoreInfo.Rank);

                Assert.That(score.Replay.Frames, Has.One.Matches<ManiaReplayFrame>(frame =>
                    frame.Time == 414 && frame.Actions.SequenceEqual(new[] { ManiaAction.Key1, ManiaAction.Key18 })));
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
            const double first_frame_time = 31;
            const double second_frame_time = 48;
            const double third_frame_time = 65;

            var decoder = new TestLegacyScoreDecoder(beatmapVersion);

            using (var resourceStream = TestResources.OpenResource("Replays/mania-replay.osr"))
            {
                var score = decoder.Parse(resourceStream);
                int offset = offsetApplied ? LegacyBeatmapDecoder.EARLY_VERSION_TIMING_OFFSET : 0;

                Assert.That(score.Replay.Frames[0].Time, Is.EqualTo(first_frame_time + offset));
                Assert.That(score.Replay.Frames[1].Time, Is.EqualTo(second_frame_time + offset));
                Assert.That(score.Replay.Frames[2].Time, Is.EqualTo(third_frame_time + offset));
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
        public void TestNegativeFrameSkipped()
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
                        new OsuReplayFrame(0, new Vector2()),
                        new OsuReplayFrame(1000, OsuPlayfield.BASE_SIZE),
                        new OsuReplayFrame(500, OsuPlayfield.BASE_SIZE / 2),
                        new OsuReplayFrame(2000, OsuPlayfield.BASE_SIZE),
                    }
                }
            };

            var decodedAfterEncode = encodeThenDecode(LegacyScoreEncoder.LATEST_VERSION, score, beatmap);

            Assert.That(decodedAfterEncode.Replay.Frames, Has.Count.EqualTo(3));
            Assert.That(decodedAfterEncode.Replay.Frames[0].Time, Is.EqualTo(0));
            Assert.That(decodedAfterEncode.Replay.Frames[1].Time, Is.EqualTo(1000));
            Assert.That(decodedAfterEncode.Replay.Frames[2].Time, Is.EqualTo(2000));
        }

        [Test]
        public void FirstTwoFramesSwappedIfInWrongOrder()
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
                        new OsuReplayFrame(100, new Vector2()),
                        new OsuReplayFrame(50, OsuPlayfield.BASE_SIZE / 2),
                        new OsuReplayFrame(1000, OsuPlayfield.BASE_SIZE),
                    }
                }
            };

            var decodedAfterEncode = encodeThenDecode(LegacyScoreEncoder.LATEST_VERSION, score, beatmap);

            Assert.That(decodedAfterEncode.Replay.Frames, Has.Count.EqualTo(3));
            Assert.That(decodedAfterEncode.Replay.Frames[0].Time, Is.EqualTo(0));
            Assert.That(decodedAfterEncode.Replay.Frames[1].Time, Is.EqualTo(100));
            Assert.That(decodedAfterEncode.Replay.Frames[2].Time, Is.EqualTo(1000));
        }

        [Test]
        public void FirstTwoFramesPulledTowardThirdIfTheyAreAfterIt()
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
                        new OsuReplayFrame(0, new Vector2()),
                        new OsuReplayFrame(500, OsuPlayfield.BASE_SIZE / 2),
                        new OsuReplayFrame(-1500, OsuPlayfield.BASE_SIZE),
                    }
                }
            };

            var decodedAfterEncode = encodeThenDecode(LegacyScoreEncoder.LATEST_VERSION, score, beatmap);

            Assert.That(decodedAfterEncode.Replay.Frames, Has.Count.EqualTo(3));
            Assert.That(decodedAfterEncode.Replay.Frames[0].Time, Is.EqualTo(-1500));
            Assert.That(decodedAfterEncode.Replay.Frames[1].Time, Is.EqualTo(-1500));
            Assert.That(decodedAfterEncode.Replay.Frames[2].Time, Is.EqualTo(-1500));
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
            scoreInfo.OnlineID = 123123;
            scoreInfo.User = new APIUser
            {
                Username = "spaceman_atlas",
                Id = 3035836,
                CountryCode = CountryCode.PL
            };
            scoreInfo.ClientVersion = "2023.1221.0";

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
                Assert.That(decodedAfterEncode.ScoreInfo.OnlineID, Is.EqualTo(123123));
                Assert.That(decodedAfterEncode.ScoreInfo.Statistics, Is.EqualTo(scoreInfo.Statistics));
                Assert.That(decodedAfterEncode.ScoreInfo.MaximumStatistics, Is.EqualTo(scoreInfo.MaximumStatistics));
                Assert.That(decodedAfterEncode.ScoreInfo.Mods, Is.EqualTo(scoreInfo.Mods));
                Assert.That(decodedAfterEncode.ScoreInfo.ClientVersion, Is.EqualTo("2023.1221.0"));
                Assert.That(decodedAfterEncode.ScoreInfo.RealmUser.OnlineID, Is.EqualTo(3035836));
            });
        }

        [Test]
        public void AccuracyOfStableScoreRecomputed()
        {
            var memoryStream = new MemoryStream();

            // local partial implementation of legacy score encoder
            // this is done half for readability, half because `LegacyScoreEncoder` forces `LATEST_VERSION`
            // and we want to emulate a stable score here
            using (var sw = new SerializationWriter(memoryStream, true))
            {
                sw.Write((byte)3); // ruleset id (mania).
                                   // mania is used intentionally as it is the only ruleset wherein default accuracy calculation is changed in lazer
                sw.Write(20240116); // version (anything below `LegacyScoreEncoder.FIRST_LAZER_VERSION` is stable)
                sw.Write(string.Empty.ComputeMD5Hash()); // beatmap hash, irrelevant to this test
                sw.Write("username"); // irrelevant to this test
                sw.Write(string.Empty.ComputeMD5Hash()); // score hash, irrelevant to this test
                sw.Write((ushort)1); // count300
                sw.Write((ushort)0); // count100
                sw.Write((ushort)0); // count50
                sw.Write((ushort)198); // countGeki (perfects / "rainbow 300s" in mania)
                sw.Write((ushort)0); // countKatu
                sw.Write((ushort)1); // countMiss
                sw.Write(12345678); // total score, irrelevant to this test
                sw.Write((ushort)1000); // max combo, irrelevant to this test
                sw.Write(false); // full combo, irrelevant to this test
                sw.Write((int)LegacyMods.Hidden); // mods
                sw.Write(string.Empty); // hp graph, irrelevant
                sw.Write(DateTime.Now); // date, irrelevant
                sw.Write(Array.Empty<byte>()); // replay data, irrelevant
                sw.Write((long)1234); // legacy online ID, irrelevant
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var decoded = new TestLegacyScoreDecoder().Parse(memoryStream);

            Assert.Multiple(() =>
            {
                Assert.That(decoded.ScoreInfo.Accuracy, Is.EqualTo((double)(198 * 305 + 300) / (200 * 305)));
                Assert.That(decoded.ScoreInfo.Rank, Is.EqualTo(ScoreRank.SH));
            });
        }

        [Test]
        public void RankOfStableScoreUsesLazerDefinitions()
        {
            var memoryStream = new MemoryStream();

            // local partial implementation of legacy score encoder
            // this is done half for readability, half because `LegacyScoreEncoder` forces `LATEST_VERSION`
            // and we want to emulate a stable score here
            using (var sw = new SerializationWriter(memoryStream, true))
            {
                sw.Write((byte)0); // ruleset id (osu!)
                sw.Write(20240116); // version (anything below `LegacyScoreEncoder.FIRST_LAZER_VERSION` is stable)
                sw.Write(string.Empty.ComputeMD5Hash()); // beatmap hash, irrelevant to this test
                sw.Write("username"); // irrelevant to this test
                sw.Write(string.Empty.ComputeMD5Hash()); // score hash, irrelevant to this test
                sw.Write((ushort)195); // count300
                sw.Write((ushort)1); // count100
                sw.Write((ushort)4); // count50
                sw.Write((ushort)0); // countGeki
                sw.Write((ushort)0); // countKatu
                sw.Write((ushort)0); // countMiss
                sw.Write(12345678); // total score, irrelevant to this test
                sw.Write((ushort)1000); // max combo, irrelevant to this test
                sw.Write(false); // full combo, irrelevant to this test
                sw.Write((int)LegacyMods.Hidden); // mods
                sw.Write(string.Empty); // hp graph, irrelevant
                sw.Write(DateTime.Now); // date, irrelevant
                sw.Write(Array.Empty<byte>()); // replay data, irrelevant
                sw.Write((long)1234); // legacy online ID, irrelevant
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var decoded = new TestLegacyScoreDecoder().Parse(memoryStream);

            Assert.Multiple(() =>
            {
                // In stable this would be an A because there are over 1% 50s. But that's not a thing in lazer.
                Assert.That(decoded.ScoreInfo.Rank, Is.EqualTo(ScoreRank.SH));
            });
        }

        [Test]
        public void AccuracyRankAndTotalScoreOfLazerScorePreserved()
        {
            var ruleset = new OsuRuleset().RulesetInfo;

            var scoreInfo = TestResources.CreateTestScoreInfo(ruleset);
            scoreInfo.Mods = new Mod[] { new OsuModFlashlight() };
            scoreInfo.Statistics = new Dictionary<HitResult, int>
            {
                [HitResult.Great] = 199,
                [HitResult.Miss] = 1,
                [HitResult.LargeTickHit] = 1,
            };
            scoreInfo.MaximumStatistics = new Dictionary<HitResult, int>
            {
                [HitResult.Great] = 200,
                [HitResult.LargeTickHit] = 1,
            };
            scoreInfo.Rank = ScoreRank.A;

            var beatmap = new TestBeatmap(ruleset);
            var score = new Score
            {
                ScoreInfo = scoreInfo,
            };

            var decodedAfterEncode = encodeThenDecode(LegacyBeatmapDecoder.LATEST_VERSION, score, beatmap);

            Assert.Multiple(() =>
            {
                Assert.That(decodedAfterEncode.ScoreInfo.TotalScore, Is.EqualTo(284_537));
                Assert.That(decodedAfterEncode.ScoreInfo.LegacyTotalScore, Is.Null);
                Assert.That(decodedAfterEncode.ScoreInfo.Accuracy, Is.EqualTo((double)(199 * 300 + 30) / (200 * 300 + 30)));
                Assert.That(decodedAfterEncode.ScoreInfo.Rank, Is.EqualTo(ScoreRank.A));
            });
        }

        [Test]
        public void AccuracyAndRankOfLazerScoreWithoutLegacyReplaySoloScoreInfoUsesBestEffortFallbackToLegacy()
        {
            var memoryStream = new MemoryStream();

            // local partial implementation of legacy score encoder
            // this is done half for readability, half because we want to emulate an old lazer score here
            // that does not have everything that `LegacyScoreEncoder` now writes to the replay
            using (var sw = new SerializationWriter(memoryStream, true))
            {
                sw.Write((byte)0); // ruleset id (osu!)
                sw.Write(LegacyScoreEncoder.FIRST_LAZER_VERSION); // version
                sw.Write(string.Empty.ComputeMD5Hash()); // beatmap hash, irrelevant to this test
                sw.Write("username"); // irrelevant to this test
                sw.Write(string.Empty.ComputeMD5Hash()); // score hash, irrelevant to this test
                sw.Write((ushort)198); // count300
                sw.Write((ushort)0); // count100
                sw.Write((ushort)1); // count50
                sw.Write((ushort)0); // countGeki
                sw.Write((ushort)0); // countKatu
                sw.Write((ushort)1); // countMiss
                sw.Write(12345678); // total score, irrelevant to this test
                sw.Write((ushort)1000); // max combo, irrelevant to this test
                sw.Write(false); // full combo, irrelevant to this test
                sw.Write((int)LegacyMods.Hidden); // mods
                sw.Write(string.Empty); // hp graph, irrelevant
                sw.Write(DateTime.Now); // date, irrelevant
                sw.Write(Array.Empty<byte>()); // replay data, irrelevant
                sw.Write((long)1234); // legacy online ID, irrelevant
                // importantly, no compressed `LegacyReplaySoloScoreInfo` here
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var decoded = new TestLegacyScoreDecoder().Parse(memoryStream);

            Assert.Multiple(() =>
            {
                Assert.That(decoded.ScoreInfo.Accuracy, Is.EqualTo((double)(198 * 300 + 50) / (200 * 300)));
                Assert.That(decoded.ScoreInfo.Rank, Is.EqualTo(ScoreRank.A));
            });
        }

        [Test]
        public void TestTotalScoreWithoutModsReadIfPresent()
        {
            var ruleset = new OsuRuleset().RulesetInfo;

            var scoreInfo = TestResources.CreateTestScoreInfo(ruleset);
            scoreInfo.Mods = new Mod[]
            {
                new OsuModDoubleTime { SpeedChange = { Value = 1.1 } }
            };
            scoreInfo.OnlineID = 123123;
            scoreInfo.ClientVersion = "2023.1221.0";
            scoreInfo.TotalScoreWithoutMods = 1_000_000;
            scoreInfo.TotalScore = 1_020_000;

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
                Assert.That(decodedAfterEncode.ScoreInfo.TotalScoreWithoutMods, Is.EqualTo(1_000_000));
                Assert.That(decodedAfterEncode.ScoreInfo.TotalScore, Is.EqualTo(1_020_000));
            });
        }

        [Test]
        public void TestTotalScoreWithoutModsBackwardsPopulatedIfMissing()
        {
            var ruleset = new OsuRuleset().RulesetInfo;

            var scoreInfo = TestResources.CreateTestScoreInfo(ruleset);
            scoreInfo.Mods = new Mod[]
            {
                new OsuModDoubleTime { SpeedChange = { Value = 1.1 } }
            };
            scoreInfo.OnlineID = 123123;
            scoreInfo.ClientVersion = "2023.1221.0";
            scoreInfo.TotalScoreWithoutMods = 0;
            scoreInfo.TotalScore = 1_020_000;

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
                Assert.That(decodedAfterEncode.ScoreInfo.TotalScoreWithoutMods, Is.EqualTo(1_000_000));
                Assert.That(decodedAfterEncode.ScoreInfo.TotalScore, Is.EqualTo(1_020_000));
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

        public class TestLegacyScoreDecoder : LegacyScoreDecoder
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
                },
                // needs to have at least one objects so that `StandardisedScoreMigrationTools` doesn't die
                // when trying to recompute total score.
                HitObjects =
                {
                    new HitCircle()
                }
            });
        }
    }
}
