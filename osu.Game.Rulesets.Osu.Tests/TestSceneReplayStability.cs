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
    public partial class TestSceneReplayStability : ReplayStabilityTestScene
    {
        private static readonly object[][] test_cases =
        {
            // With respect to notation,
            // square brackets `[]` represent *closed* or *inclusive* bounds,
            // while round brackets `()` represent *open* or *exclusive* bounds.

            // OD = 5 test cases.
            // GREAT hit window is [ -49.5ms,  49.5ms]
            // OK    hit window is [ -99.5ms,  99.5ms]
            // MEH   hit window is [-149.5ms, 149.5ms]
            new object[] { 5f, 49d, HitResult.Great },
            new object[] { 5f, 49.2d, HitResult.Great },
            new object[] { 5f, 49.7d, HitResult.Ok },
            new object[] { 5f, 50d, HitResult.Ok },
            new object[] { 5f, 50.4d, HitResult.Ok },
            new object[] { 5f, 50.9d, HitResult.Ok },
            new object[] { 5f, 51d, HitResult.Ok },
            new object[] { 5f, 99d, HitResult.Ok },
            new object[] { 5f, 99.2d, HitResult.Ok },
            new object[] { 5f, 99.7d, HitResult.Meh },
            new object[] { 5f, 100d, HitResult.Meh },
            new object[] { 5f, 100.4d, HitResult.Meh },
            new object[] { 5f, 100.9d, HitResult.Meh },
            new object[] { 5f, 101d, HitResult.Meh },
            new object[] { 5f, 149d, HitResult.Meh },
            new object[] { 5f, 149.2d, HitResult.Meh },
            new object[] { 5f, 149.7d, HitResult.Miss },
            new object[] { 5f, 150d, HitResult.Miss },
            new object[] { 5f, 150.4d, HitResult.Miss },
            new object[] { 5f, 150.9d, HitResult.Miss },
            new object[] { 5f, 151d, HitResult.Miss },

            // OD = 5.7 test cases.
            // GREAT hit window is [ -44.5ms,  44.5ms]
            // OK    hit window is [ -93.5ms,  93.5ms]
            // MEH   hit window is [-142.5ms, 142.5ms]
            new object[] { 5.7f, 44d, HitResult.Great },
            new object[] { 5.7f, 44.2d, HitResult.Great },
            new object[] { 5.7f, 44.8d, HitResult.Ok },
            new object[] { 5.7f, 45d, HitResult.Ok },
            new object[] { 5.7f, 45.4d, HitResult.Ok },
            new object[] { 5.7f, 93d, HitResult.Ok },
            new object[] { 5.7f, 93.4d, HitResult.Ok },
            new object[] { 5.7f, 93.9d, HitResult.Meh },
            new object[] { 5.7f, 94d, HitResult.Meh },
            new object[] { 5.7f, 94.4d, HitResult.Meh },
            new object[] { 5.7f, 142d, HitResult.Meh },
            new object[] { 5.7f, 142.2d, HitResult.Meh },
            new object[] { 5.7f, 142.7d, HitResult.Miss },
            new object[] { 5.7f, 143d, HitResult.Miss },
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
