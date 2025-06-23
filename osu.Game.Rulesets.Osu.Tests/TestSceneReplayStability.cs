// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [Ignore("These tests are expected to fail until an acceptable solution for various replay playback issues concerning rounding of replay frame times & hit windows is found.")]
    public partial class TestSceneReplayStability : ReplayStabilityTestScene
    {
        private static readonly object[][] test_cases =
        {
            // With respect to notation,
            // square brackets `[]` represent *closed* or *inclusive* bounds,
            // while round brackets `()` represent *open* or *exclusive* bounds.

            // OD = 5 test cases.
            // GREAT hit window is [ -50ms,  50ms]
            // OK    hit window is [-100ms, 100ms]
            // MEH   hit window is [-150ms, 150ms]
            // MISS  hit window is [-400ms, 400ms]
            new object[] { 5f, 49d, HitResult.Great },
            new object[] { 5f, 49.2d, HitResult.Great },
            new object[] { 5f, 49.7d, HitResult.Great },
            new object[] { 5f, 50d, HitResult.Great },
            new object[] { 5f, 50.4d, HitResult.Ok },
            new object[] { 5f, 50.9d, HitResult.Ok },
            new object[] { 5f, 51d, HitResult.Ok },
            new object[] { 5f, 99d, HitResult.Ok },
            new object[] { 5f, 99.2d, HitResult.Ok },
            new object[] { 5f, 99.7d, HitResult.Ok },
            new object[] { 5f, 100d, HitResult.Ok },
            new object[] { 5f, 100.4d, HitResult.Meh },
            new object[] { 5f, 100.9d, HitResult.Meh },
            new object[] { 5f, 101d, HitResult.Meh },
            new object[] { 5f, 149d, HitResult.Meh },
            new object[] { 5f, 149.2d, HitResult.Meh },
            new object[] { 5f, 149.7d, HitResult.Meh },
            new object[] { 5f, 150d, HitResult.Meh },
            new object[] { 5f, 150.4d, HitResult.Miss },
            new object[] { 5f, 150.9d, HitResult.Miss },
            new object[] { 5f, 151d, HitResult.Miss },

            // OD = 5.7 test cases.
            // GREAT hit window is [ -45.8ms,  45.8ms]
            // OK    hit window is [ -94.4ms,  94.4ms]
            // MEH   hit window is [-143.0ms, 143.0ms]
            // MISS  hit window is [-400.0ms, 400.0ms]
            new object[] { 5.7f, 45d, HitResult.Great },
            new object[] { 5.7f, 45.2d, HitResult.Great },
            new object[] { 5.7f, 45.8d, HitResult.Great },
            new object[] { 5.7f, 45.9d, HitResult.Ok },
            new object[] { 5.7f, 46d, HitResult.Ok },
            new object[] { 5.7f, 46.4d, HitResult.Ok },
            new object[] { 5.7f, 94d, HitResult.Ok },
            new object[] { 5.7f, 94.2d, HitResult.Ok },
            new object[] { 5.7f, 94.4d, HitResult.Ok },
            new object[] { 5.7f, 94.48d, HitResult.Ok },
            new object[] { 5.7f, 94.9d, HitResult.Meh },
            new object[] { 5.7f, 95d, HitResult.Meh },
            new object[] { 5.7f, 95.4d, HitResult.Meh },
            new object[] { 5.7f, 142d, HitResult.Meh },
            new object[] { 5.7f, 142.7d, HitResult.Meh },
            new object[] { 5.7f, 143d, HitResult.Meh },
            new object[] { 5.7f, 143.4d, HitResult.Miss },
            new object[] { 5.7f, 143.9d, HitResult.Miss },
            new object[] { 5.7f, 144d, HitResult.Miss },
        };

        [TestCaseSource(nameof(test_cases))]
        public void TestHitWindowStability(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            const double hit_circle_time = 100;

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
            };

            var replay = new Replay
            {
                Frames =
                {
                    new OsuReplayFrame(0, OsuPlayfield.BASE_SIZE / 2),
                    new OsuReplayFrame(hit_circle_time + hitOffset, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton),
                    new OsuReplayFrame(hit_circle_time + hitOffset + 20, OsuPlayfield.BASE_SIZE / 2),
                }
            };

            RunTest(beatmap, replay, [expectedResult]);
        }
    }
}
