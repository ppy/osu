// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests
{
    [Ignore("These tests are expected to fail until an acceptable solution for various replay playback issues concerning rounding of replay frame times & hit windows is found.")]
    public partial class TestSceneLegacyReplayPlayback : LegacyReplayPlaybackTestScene
    {
        protected override Ruleset CreateRuleset() => new ManiaRuleset();

        protected override string? ExportLocation => null;

        private static readonly object[][] score_v2_test_cases =
        {
            // With respect to notation,
            // square brackets `[]` represent *closed* or *inclusive* bounds,
            // while round brackets `()` represent *open* or *exclusive* bounds.

            // Note that mania hitwindows are heavily idiosyncratic,
            // and if you *think* a number here is wrong, probably double check.

            // Known issues / complexities:
            // - There is a disparate set of hitwindow ranges for: score V1 non-converts, score V1 converts, and score V2 (regardless of convert)
            // - It is NEVER POSSIBLE to get a MEH result when late; exceeding the OK hit windows will result in a MISS.
            //   Additionally, the OK hit window when late is EXCLUSIVE / OPEN rather than INCLUSIVE / CLOSED.
            //   Relevant stable source: https://github.com/peppy/osu-stable-reference/blob/996648fba06baf4e7d2e0b248959399444017895/osu!/GameplayElements/HitObjectManagerMania.cs#L737-L751
            // - There is also a seemingly mania-specific issue wherein key inputs registered before time instant 0 get truncated to time 0,
            //   which is why the beatmaps used below make sure not to cross that boundary (the note starts at t=300ms).
            //   This is not an issue in osu! or taiko.
            //   The source of this behaviour has not been investigated in detail.

            // OD = 5 test cases.
            // PERFECT hit window is [ -19ms,  19ms]
            // GREAT   hit window is [ -49ms,  49ms]
            // GOOD    hit window is [ -82ms,  82ms]
            // OK      hit window is [-112ms, 112ms) <- not a typo, this side of the interval is OPEN!
            // MEH     hit window is [-136ms, -----) <- it is NOT POSSIBLE to get a MEH result on a late hit!
            new object[] { 5f, -18d, HitResult.Perfect },
            new object[] { 5f, -19d, HitResult.Perfect },
            new object[] { 5f, -20d, HitResult.Great },
            new object[] { 5f, -21d, HitResult.Great },
            new object[] { 5f, -48d, HitResult.Great },
            new object[] { 5f, -49d, HitResult.Great },
            new object[] { 5f, -50d, HitResult.Good },
            new object[] { 5f, -51d, HitResult.Good },
            new object[] { 5f, -81d, HitResult.Good },
            new object[] { 5f, -82d, HitResult.Good },
            new object[] { 5f, -83d, HitResult.Ok },
            new object[] { 5f, -84d, HitResult.Ok },
            new object[] { 5f, -111d, HitResult.Ok },
            new object[] { 5f, -112d, HitResult.Ok },
            new object[] { 5f, -113d, HitResult.Meh },
            new object[] { 5f, -114d, HitResult.Meh },
            new object[] { 5f, -135d, HitResult.Meh },
            new object[] { 5f, -136d, HitResult.Meh },
            new object[] { 5f, -137d, HitResult.Miss },
            new object[] { 5f, -138d, HitResult.Miss },
            new object[] { 5f, 111d, HitResult.Ok },
            new object[] { 5f, 112d, HitResult.Miss },
            new object[] { 5f, 113d, HitResult.Miss },
            new object[] { 5f, 114d, HitResult.Miss },
            new object[] { 5f, 135d, HitResult.Miss },
            new object[] { 5f, 136d, HitResult.Miss },
            new object[] { 5f, 137d, HitResult.Miss },
            new object[] { 5f, 138d, HitResult.Miss },

            // OD = 9.3 test cases.
            // PERFECT hit window is [ -14ms,  14ms]
            // GREAT   hit window is [ -36ms,  36ms]
            // GOOD    hit window is [ -69ms,  69ms]
            // OK      hit window is [ -99ms,  99ms) <- not a typo, this side of the interval is OPEN!
            // MEH     hit window is [-123ms, -----) <- it is NOT POSSIBLE to get a MEH result on a late hit!
            new object[] { 9.3f, 13d, HitResult.Perfect },
            new object[] { 9.3f, 14d, HitResult.Perfect },
            new object[] { 9.3f, 15d, HitResult.Great },
            new object[] { 9.3f, 16d, HitResult.Great },
            new object[] { 9.3f, 35d, HitResult.Great },
            new object[] { 9.3f, 36d, HitResult.Great },
            new object[] { 9.3f, 37d, HitResult.Good },
            new object[] { 9.3f, 38d, HitResult.Good },
            new object[] { 9.3f, 68d, HitResult.Good },
            new object[] { 9.3f, 69d, HitResult.Good },
            new object[] { 9.3f, 70d, HitResult.Ok },
            new object[] { 9.3f, 71d, HitResult.Ok },
            new object[] { 9.3f, 98d, HitResult.Ok },
            new object[] { 9.3f, 99d, HitResult.Miss },
            new object[] { 9.3f, 100d, HitResult.Miss },
            new object[] { 9.3f, 101d, HitResult.Miss },
            new object[] { 9.3f, 122d, HitResult.Miss },
            new object[] { 9.3f, 123d, HitResult.Miss },
            new object[] { 9.3f, 124d, HitResult.Miss },
            new object[] { 9.3f, 125d, HitResult.Miss },
            new object[] { 9.3f, -98d, HitResult.Ok },
            new object[] { 9.3f, -99d, HitResult.Ok },
            new object[] { 9.3f, -100d, HitResult.Meh },
            new object[] { 9.3f, -101d, HitResult.Meh },
            new object[] { 9.3f, -122d, HitResult.Meh },
            new object[] { 9.3f, -123d, HitResult.Meh },
            new object[] { 9.3f, -124d, HitResult.Miss },
            new object[] { 9.3f, -125d, HitResult.Miss },
        };

        private static readonly object[][] score_v1_non_convert_test_cases =
        {
            // OD = 5 test cases.
            // PERFECT hit window is [ -16ms,  16ms]
            // GREAT   hit window is [ -49ms,  49ms]
            // GOOD    hit window is [ -82ms,  82ms]
            // OK      hit window is [-112ms, 112ms) <- not a typo, this side of the interval is OPEN!
            // MEH     hit window is [-136ms, -----) <- it is NOT POSSIBLE to get a MEH result on a late hit!
            new object[] { 5f, -15d, HitResult.Perfect },
            new object[] { 5f, -16d, HitResult.Perfect },
            new object[] { 5f, -17d, HitResult.Great },
            new object[] { 5f, -18d, HitResult.Great },
            new object[] { 5f, -48d, HitResult.Great },
            new object[] { 5f, -49d, HitResult.Great },
            new object[] { 5f, -50d, HitResult.Good },
            new object[] { 5f, -51d, HitResult.Good },
            new object[] { 5f, -81d, HitResult.Good },
            new object[] { 5f, -82d, HitResult.Good },
            new object[] { 5f, -83d, HitResult.Ok },
            new object[] { 5f, -84d, HitResult.Ok },
            new object[] { 5f, -111d, HitResult.Ok },
            new object[] { 5f, -112d, HitResult.Ok },
            new object[] { 5f, -113d, HitResult.Meh },
            new object[] { 5f, -114d, HitResult.Meh },
            new object[] { 5f, -135d, HitResult.Meh },
            new object[] { 5f, -136d, HitResult.Meh },
            new object[] { 5f, -137d, HitResult.Miss },
            new object[] { 5f, -138d, HitResult.Miss },
            new object[] { 5f, 111d, HitResult.Ok },
            new object[] { 5f, 112d, HitResult.Miss },
            new object[] { 5f, 113d, HitResult.Miss },
            new object[] { 5f, 114d, HitResult.Miss },
            new object[] { 5f, 135d, HitResult.Miss },
            new object[] { 5f, 136d, HitResult.Miss },
            new object[] { 5f, 137d, HitResult.Miss },
            new object[] { 5f, 138d, HitResult.Miss },

            // OD = 9.3 test cases.
            // PERFECT hit window is [ -16ms,  16ms]
            // GREAT   hit window is [ -36ms,  36ms]
            // GOOD    hit window is [ -69ms,  69ms]
            // OK      hit window is [ -99ms,  99ms) <- not a typo, this side of the interval is OPEN!
            // MEH     hit window is [-123ms, -----) <- it is NOT POSSIBLE to get a MEH result on a late hit!
            new object[] { 9.3f, 15d, HitResult.Perfect },
            new object[] { 9.3f, 16d, HitResult.Perfect },
            new object[] { 9.3f, 17d, HitResult.Great },
            new object[] { 9.3f, 18d, HitResult.Great },
            new object[] { 9.3f, 35d, HitResult.Great },
            new object[] { 9.3f, 36d, HitResult.Great },
            new object[] { 9.3f, 37d, HitResult.Good },
            new object[] { 9.3f, 38d, HitResult.Good },
            new object[] { 9.3f, 68d, HitResult.Good },
            new object[] { 9.3f, 69d, HitResult.Good },
            new object[] { 9.3f, 70d, HitResult.Ok },
            new object[] { 9.3f, 71d, HitResult.Ok },
            new object[] { 9.3f, 98d, HitResult.Ok },
            new object[] { 9.3f, 99d, HitResult.Miss },
            new object[] { 9.3f, 100d, HitResult.Miss },
            new object[] { 9.3f, 101d, HitResult.Miss },
            new object[] { 9.3f, 122d, HitResult.Miss },
            new object[] { 9.3f, 123d, HitResult.Miss },
            new object[] { 9.3f, 124d, HitResult.Miss },
            new object[] { 9.3f, 125d, HitResult.Miss },
            new object[] { 9.3f, -98d, HitResult.Ok },
            new object[] { 9.3f, -99d, HitResult.Ok },
            new object[] { 9.3f, -100d, HitResult.Meh },
            new object[] { 9.3f, -101d, HitResult.Meh },
            new object[] { 9.3f, -122d, HitResult.Meh },
            new object[] { 9.3f, -123d, HitResult.Meh },
            new object[] { 9.3f, -124d, HitResult.Miss },
            new object[] { 9.3f, -125d, HitResult.Miss },

            // OD = 3.1 test cases.
            // PERFECT hit window is [ -16ms,  16ms]
            // GREAT   hit window is [ -54ms,  54ms]
            // GOOD    hit window is [ -87ms,  87ms]
            // OK      hit window is [-117ms, 117ms) <- not a typo, this side of the interval is OPEN!
            // MEH     hit window is [-141ms, -----) <- it is NOT POSSIBLE to get a MEH result on a late hit!
            new object[] { 3.1f, 15d, HitResult.Perfect },
            new object[] { 3.1f, 16d, HitResult.Perfect },
            new object[] { 3.1f, 17d, HitResult.Great },
            new object[] { 3.1f, 18d, HitResult.Great },
            new object[] { 3.1f, 53d, HitResult.Great },
            new object[] { 3.1f, 54d, HitResult.Great },
            new object[] { 3.1f, 55d, HitResult.Good },
            new object[] { 3.1f, 56d, HitResult.Good },
            new object[] { 3.1f, 86d, HitResult.Good },
            new object[] { 3.1f, 87d, HitResult.Good },
            new object[] { 3.1f, 88d, HitResult.Ok },
            new object[] { 3.1f, 89d, HitResult.Ok },
            new object[] { 3.1f, 116d, HitResult.Ok },
            new object[] { 3.1f, 117d, HitResult.Miss },
            new object[] { 3.1f, 118d, HitResult.Miss },
            new object[] { 3.1f, 119d, HitResult.Miss },
            new object[] { 3.1f, 140d, HitResult.Miss },
            new object[] { 3.1f, 141d, HitResult.Miss },
            new object[] { 3.1f, 142d, HitResult.Miss },
            new object[] { 3.1f, 143d, HitResult.Miss },
            new object[] { 3.1f, -116d, HitResult.Ok },
            new object[] { 3.1f, -117d, HitResult.Ok },
            new object[] { 3.1f, -118d, HitResult.Meh },
            new object[] { 3.1f, -119d, HitResult.Meh },
            new object[] { 3.1f, -140d, HitResult.Meh },
            new object[] { 3.1f, -141d, HitResult.Meh },
            new object[] { 3.1f, -142d, HitResult.Miss },
            new object[] { 3.1f, -143d, HitResult.Miss },
        };

        private static readonly object[][] score_v1_convert_test_cases =
        {
            // OD = 5 test cases.
            // PERFECT hit window is [ -16ms,  16ms]
            // GREAT   hit window is [ -34ms,  34ms]
            // GOOD    hit window is [ -67ms,  67ms]
            // OK      hit window is [ -97ms,  97ms) <- not a typo, this side of the interval is OPEN!
            // MEH     hit window is [-121ms, -----) <- it is NOT POSSIBLE to get a MEH result on a late hit!
            new object[] { 5f, -15d, HitResult.Perfect },
            new object[] { 5f, -16d, HitResult.Perfect },
            new object[] { 5f, -17d, HitResult.Great },
            new object[] { 5f, -18d, HitResult.Great },
            new object[] { 5f, -33d, HitResult.Great },
            new object[] { 5f, -34d, HitResult.Great },
            new object[] { 5f, -35d, HitResult.Good },
            new object[] { 5f, -36d, HitResult.Good },
            new object[] { 5f, -66d, HitResult.Good },
            new object[] { 5f, -67d, HitResult.Good },
            new object[] { 5f, -68d, HitResult.Ok },
            new object[] { 5f, -69d, HitResult.Ok },
            new object[] { 5f, -96d, HitResult.Ok },
            new object[] { 5f, -97d, HitResult.Ok },
            new object[] { 5f, -98d, HitResult.Meh },
            new object[] { 5f, -99d, HitResult.Meh },
            new object[] { 5f, -120d, HitResult.Meh },
            new object[] { 5f, -121d, HitResult.Meh },
            new object[] { 5f, -122d, HitResult.Miss },
            new object[] { 5f, -123d, HitResult.Miss },
            new object[] { 5f, 96d, HitResult.Ok },
            new object[] { 5f, 97d, HitResult.Miss },
            new object[] { 5f, 98d, HitResult.Miss },
            new object[] { 5f, 99d, HitResult.Miss },
            new object[] { 5f, 120d, HitResult.Miss },
            new object[] { 5f, 121d, HitResult.Miss },
            new object[] { 5f, 122d, HitResult.Miss },
            new object[] { 5f, 123d, HitResult.Miss },

            // OD = 3.1 test cases.
            // PERFECT hit window is [ -16ms,  16ms]
            // GREAT   hit window is [ -47ms,  47ms]
            // GOOD    hit window is [ -77ms,  77ms]
            // OK      hit window is [ -97ms,  97ms) <- not a typo, this side of the interval is OPEN!
            // MEH     hit window is [-121ms, -----) <- it is NOT POSSIBLE to get a MEH result on a late hit!
            new object[] { 3.1f, 15d, HitResult.Perfect },
            new object[] { 3.1f, 16d, HitResult.Perfect },
            new object[] { 3.1f, 17d, HitResult.Great },
            new object[] { 3.1f, 18d, HitResult.Great },
            new object[] { 3.1f, 46d, HitResult.Great },
            new object[] { 3.1f, 47d, HitResult.Great },
            new object[] { 3.1f, 48d, HitResult.Good },
            new object[] { 3.1f, 49d, HitResult.Good },
            new object[] { 3.1f, 76d, HitResult.Good },
            new object[] { 3.1f, 77d, HitResult.Good },
            new object[] { 3.1f, 78d, HitResult.Ok },
            new object[] { 3.1f, 79d, HitResult.Ok },
            new object[] { 3.1f, 96d, HitResult.Ok },
            new object[] { 3.1f, 97d, HitResult.Miss },
            new object[] { 3.1f, 98d, HitResult.Miss },
            new object[] { 3.1f, 99d, HitResult.Miss },
            new object[] { 3.1f, 120d, HitResult.Miss },
            new object[] { 3.1f, 121d, HitResult.Miss },
            new object[] { 3.1f, 122d, HitResult.Miss },
            new object[] { 3.1f, 123d, HitResult.Miss },
            new object[] { 3.1f, -96d, HitResult.Ok },
            new object[] { 3.1f, -97d, HitResult.Ok },
            new object[] { 3.1f, -98d, HitResult.Meh },
            new object[] { 3.1f, -99d, HitResult.Meh },
            new object[] { 3.1f, -120d, HitResult.Meh },
            new object[] { 3.1f, -121d, HitResult.Meh },
            new object[] { 3.1f, -122d, HitResult.Miss },
            new object[] { 3.1f, -123d, HitResult.Miss },
        };

        [TestCaseSource(nameof(score_v2_test_cases))]
        public void TestHitWindowTreatmentWithScoreV2(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            const double note_time = 300;

            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 1000 });
            var beatmap = new ManiaBeatmap(new StageDefinition(1))
            {
                HitObjects =
                {
                    new Note
                    {
                        StartTime = note_time,
                        Column = 0,
                    }
                },
                Difficulty = new BeatmapDifficulty
                {
                    OverallDifficulty = overallDifficulty,
                    CircleSize = 1,
                },
                BeatmapInfo =
                {
                    Ruleset = new ManiaRuleset().RulesetInfo,
                },
                ControlPointInfo = cpi,
            };

            var replay = new Replay
            {
                Frames =
                {
                    new ManiaReplayFrame(0),
                    new ManiaReplayFrame(note_time + hitOffset, ManiaAction.Key1),
                    new ManiaReplayFrame(note_time + hitOffset + 20),
                }
            };

            var score = new Score
            {
                Replay = replay,
                ScoreInfo = new ScoreInfo
                {
                    Ruleset = CreateRuleset().RulesetInfo,
                    Mods = [new ModScoreV2()]
                }
            };

            RunTest($@"SV2 single note @ OD{overallDifficulty}", beatmap, $@"SV2 {hitOffset}ms @ OD{overallDifficulty} = {expectedResult}", score, [expectedResult]);
        }

        [TestCaseSource(nameof(score_v1_non_convert_test_cases))]
        public void TestHitWindowTreatmentWithScoreV1NonConvert(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            const double note_time = 300;

            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 1000 });
            var beatmap = new ManiaBeatmap(new StageDefinition(1))
            {
                HitObjects =
                {
                    new Note
                    {
                        StartTime = note_time,
                        Column = 0,
                    }
                },
                Difficulty = new BeatmapDifficulty
                {
                    OverallDifficulty = overallDifficulty,
                    CircleSize = 1,
                },
                BeatmapInfo =
                {
                    Ruleset = new ManiaRuleset().RulesetInfo,
                },
                ControlPointInfo = cpi,
            };

            var replay = new Replay
            {
                Frames =
                {
                    new ManiaReplayFrame(0),
                    new ManiaReplayFrame(note_time + hitOffset, ManiaAction.Key1),
                    new ManiaReplayFrame(note_time + hitOffset + 20),
                }
            };

            var score = new Score
            {
                Replay = replay,
                ScoreInfo = new ScoreInfo
                {
                    Ruleset = CreateRuleset().RulesetInfo,
                }
            };

            RunTest($@"SV1 single note @ OD{overallDifficulty}", beatmap, $@"SV1 {hitOffset}ms @ OD{overallDifficulty} = {expectedResult}", score, [expectedResult]);
        }

        [TestCaseSource(nameof(score_v1_convert_test_cases))]
        public void TestHitWindowTreatmentWithScoreV1Convert(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            const double note_time = 300;

            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 1000 });
            var beatmap = new Beatmap
            {
                HitObjects =
                {
                    new FakeCircle
                    {
                        StartTime = note_time,
                    }
                },
                Difficulty = new BeatmapDifficulty
                {
                    OverallDifficulty = overallDifficulty,
                },
                BeatmapInfo =
                {
                    Ruleset = new RulesetInfo { OnlineID = 0 }
                },
                ControlPointInfo = cpi,
            };

            var replay = new Replay
            {
                Frames =
                {
                    new ManiaReplayFrame(0),
                    new ManiaReplayFrame(note_time + hitOffset, ManiaAction.Key1),
                    new ManiaReplayFrame(note_time + hitOffset + 20),
                }
            };

            var score = new Score
            {
                Replay = replay,
                ScoreInfo = new ScoreInfo
                {
                    Ruleset = CreateRuleset().RulesetInfo,
                    Mods = [new ManiaModKey1()],
                }
            };

            RunTest($@"SV1 convert single note @ OD{overallDifficulty}", beatmap, $@"SV1 convert {hitOffset}ms @ OD{overallDifficulty} = {expectedResult}", score, [expectedResult]);
        }

        private class FakeCircle : HitObject, IHasPosition
        {
            public float X
            {
                get => Position.X;
                set => Position = new Vector2(value, Position.Y);
            }

            public float Y
            {
                get => Position.Y;
                set => Position = new Vector2(Position.X, value);
            }

            public Vector2 Position { get; set; }
        }
    }
}
