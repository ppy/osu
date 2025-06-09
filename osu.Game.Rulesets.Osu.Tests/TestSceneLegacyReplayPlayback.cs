// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [Ignore("These tests are expected to fail until an acceptable solution for various replay playback issues concerning rounding of replay frame times & hit windows is found.")]
    public partial class TestSceneLegacyReplayPlayback : LegacyReplayPlaybackTestScene
    {
        protected override Ruleset CreateRuleset() => new OsuRuleset();

        protected override string? ExportLocation => null;

        private static readonly object[][] no_mod_test_cases =
        {
            // With respect to notation,
            // square brackets `[]` represent *closed* or *inclusive* bounds,
            // while round brackets `()` represent *open* or *exclusive* bounds.
            // Additionally, note that offsets provided in double will be rounded to the nearest integer.

            // OD = 5 test cases.
            // GREAT hit window is ( -50ms,  50ms)
            // OK    hit window is (-100ms, 100ms)
            // MEH   hit window is (-150ms, 150ms)
            new object[] { 5f, 48d, HitResult.Great },
            new object[] { 5f, 49d, HitResult.Great },
            new object[] { 5f, 50d, HitResult.Ok },
            new object[] { 5f, 51d, HitResult.Ok },
            new object[] { 5f, 98d, HitResult.Ok },
            new object[] { 5f, 99d, HitResult.Ok },
            new object[] { 5f, 100d, HitResult.Meh },
            new object[] { 5f, 101d, HitResult.Meh },
            new object[] { 5f, 148d, HitResult.Meh },
            new object[] { 5f, 149d, HitResult.Meh },
            new object[] { 5f, 150d, HitResult.Miss },
            new object[] { 5f, 151d, HitResult.Miss },

            // OD = 5.7 test cases.
            // GREAT hit window is ( -45ms,  45ms)
            // OK    hit window is ( -94ms,  94ms)
            // MEH   hit window is (-143ms, 143ms)
            new object[] { 5.7f, 43d, HitResult.Great },
            new object[] { 5.7f, 44d, HitResult.Great },
            new object[] { 5.7f, 45d, HitResult.Ok },
            new object[] { 5.7f, 46d, HitResult.Ok },
            new object[] { 5.7f, 92d, HitResult.Ok },
            new object[] { 5.7f, 93d, HitResult.Ok },
            new object[] { 5.7f, 94d, HitResult.Meh },
            new object[] { 5.7f, 95d, HitResult.Meh },
            new object[] { 5.7f, 141d, HitResult.Meh },
            new object[] { 5.7f, 142d, HitResult.Meh },
            new object[] { 5.7f, 143d, HitResult.Miss },
            new object[] { 5.7f, 144d, HitResult.Miss },
        };

        private static readonly object[][] hard_rock_test_cases =
        {
            // OD = 5 test cases.
            // This leads to "effective" OD of 7.
            // GREAT hit window is ( -38ms,  38ms)
            // OK    hit window is ( -84ms,  84ms)
            // MEH   hit window is (-130ms, 130ms)
            new object[] { 5f, 36d, HitResult.Great },
            new object[] { 5f, 37d, HitResult.Great },
            new object[] { 5f, 38d, HitResult.Ok },
            new object[] { 5f, 39d, HitResult.Ok },
            new object[] { 5f, 82d, HitResult.Ok },
            new object[] { 5f, 83d, HitResult.Ok },
            new object[] { 5f, 84d, HitResult.Meh },
            new object[] { 5f, 85d, HitResult.Meh },
            new object[] { 5f, 128d, HitResult.Meh },
            new object[] { 5f, 129d, HitResult.Meh },
            new object[] { 5f, 130d, HitResult.Miss },
            new object[] { 5f, 131d, HitResult.Miss },

            // OD = 8 test cases.
            // This would lead to "effective" OD of 11.2,
            // but the effects are capped to OD 10.
            // GREAT hit window is ( -20ms,  20ms)
            // OK    hit window is ( -60ms,  60ms)
            // MEH   hit window is (-100ms, 100ms)
            new object[] { 8f, 18d, HitResult.Great },
            new object[] { 8f, 19d, HitResult.Great },
            new object[] { 8f, 20d, HitResult.Ok },
            new object[] { 8f, 21d, HitResult.Ok },
            new object[] { 8f, 58d, HitResult.Ok },
            new object[] { 8f, 59d, HitResult.Ok },
            new object[] { 8f, 60d, HitResult.Meh },
            new object[] { 8f, 61d, HitResult.Meh },
            new object[] { 8f, 98d, HitResult.Meh },
            new object[] { 8f, 99d, HitResult.Meh },
            new object[] { 8f, 100d, HitResult.Miss },
            new object[] { 8f, 101d, HitResult.Miss },
        };

        private static readonly object[][] easy_test_cases =
        {
            // OD = 5 test cases.
            // This leads to "effective" OD of 2.5.
            // GREAT hit window is ( -65ms,  65ms)
            // OK    hit window is (-120ms, 120ms)
            // MEH   hit window is (-175ms, 175ms)
            new object[] { 5f, 63d, HitResult.Great },
            new object[] { 5f, 64d, HitResult.Great },
            new object[] { 5f, 65d, HitResult.Ok },
            new object[] { 5f, 66d, HitResult.Ok },
            new object[] { 5f, 118d, HitResult.Ok },
            new object[] { 5f, 119d, HitResult.Ok },
            new object[] { 5f, 120d, HitResult.Meh },
            new object[] { 5f, 121d, HitResult.Meh },
            new object[] { 5f, 173d, HitResult.Meh },
            new object[] { 5f, 174d, HitResult.Meh },
            new object[] { 5f, 175d, HitResult.Miss },
            new object[] { 5f, 176d, HitResult.Miss },
        };

        private const double hit_circle_time = 100;

        [TestCaseSource(nameof(no_mod_test_cases))]
        public void TestHitWindowTreatment(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            var beatmap = createBeatmap(overallDifficulty);

            var replay = new Replay
            {
                Frames =
                {
                    // required for correct playback in stable
                    new OsuReplayFrame(0, new Vector2(256, -500)),
                    new OsuReplayFrame(0, new Vector2(256, -500)),
                    new OsuReplayFrame(0, OsuPlayfield.BASE_SIZE / 2),
                    new OsuReplayFrame(hit_circle_time + hitOffset, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton),
                    new OsuReplayFrame(hit_circle_time + hitOffset + 20, OsuPlayfield.BASE_SIZE / 2),
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

            RunTest($@"single circle @ OD{overallDifficulty}", beatmap, $@"{hitOffset}ms @ OD{overallDifficulty} = {expectedResult}", score, [expectedResult]);
        }

        [TestCaseSource(nameof(hard_rock_test_cases))]
        public void TestHitWindowTreatmentWithHardRock(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            var beatmap = createBeatmap(overallDifficulty);

            var replay = new Replay
            {
                Frames =
                {
                    // required for correct playback in stable
                    new OsuReplayFrame(0, new Vector2(256, -500)),
                    new OsuReplayFrame(0, new Vector2(256, -500)),
                    new OsuReplayFrame(0, OsuPlayfield.BASE_SIZE / 2),
                    new OsuReplayFrame(hit_circle_time + hitOffset, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton),
                    new OsuReplayFrame(hit_circle_time + hitOffset + 20, OsuPlayfield.BASE_SIZE / 2),
                }
            };

            var score = new Score
            {
                Replay = replay,
                ScoreInfo = new ScoreInfo
                {
                    Ruleset = CreateRuleset().RulesetInfo,
                    Mods = [new OsuModHardRock()]
                }
            };

            RunTest($@"HR single circle @ OD{overallDifficulty}", beatmap, $@"HR {hitOffset}ms @ OD{overallDifficulty} = {expectedResult}", score, [expectedResult]);
        }

        [TestCaseSource(nameof(easy_test_cases))]
        public void TestHitWindowTreatmentWithEasy(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            var beatmap = createBeatmap(overallDifficulty);

            var replay = new Replay
            {
                Frames =
                {
                    // required for correct playback in stable
                    new OsuReplayFrame(0, new Vector2(256, -500)),
                    new OsuReplayFrame(0, new Vector2(256, -500)),
                    new OsuReplayFrame(0, OsuPlayfield.BASE_SIZE / 2),
                    new OsuReplayFrame(hit_circle_time + hitOffset, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton),
                    new OsuReplayFrame(hit_circle_time + hitOffset + 20, OsuPlayfield.BASE_SIZE / 2),
                }
            };

            var score = new Score
            {
                Replay = replay,
                ScoreInfo = new ScoreInfo
                {
                    Ruleset = CreateRuleset().RulesetInfo,
                    Mods = [new OsuModEasy()]
                }
            };

            RunTest($@"EZ single circle @ OD{overallDifficulty}", beatmap, $@"EZ {hitOffset}ms @ OD{overallDifficulty} = {expectedResult}", score, [expectedResult]);
        }

        private static OsuBeatmap createBeatmap(float overallDifficulty)
        {
            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 1000 });
            var beatmap = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle
                    {
                        StartTime = hit_circle_time,
                        Position = OsuPlayfield.BASE_SIZE / 2
                    }
                },
                Difficulty = new BeatmapDifficulty { OverallDifficulty = overallDifficulty },
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo,
                },
                ControlPointInfo = cpi,
            };
            return beatmap;
        }
    }
}
