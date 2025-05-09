// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneReplayStability : ReplayStabilityTestScene
    {
        private static readonly object[][] test_cases =
        {
            // With respect to notation,
            // square brackets `[]` represent *closed* or *inclusive* bounds,
            // while round brackets `()` represent *open* or *exclusive* bounds.

            // OD = 5 test cases.
            // GREAT hit window is [-34.5ms, 34.5ms]
            // OK    hit window is [-79.5ms, 79.5ms]
            // MISS  hit window is [-94.5ms, 94.5ms]
            new object[] { 5f, -34d, HitResult.Great },
            new object[] { 5f, -34.2d, HitResult.Great },
            new object[] { 5f, -34.7d, HitResult.Ok },
            new object[] { 5f, -35d, HitResult.Ok },
            new object[] { 5f, -35.2d, HitResult.Ok },
            new object[] { 5f, -35.8d, HitResult.Ok },
            new object[] { 5f, -36d, HitResult.Ok },
            new object[] { 5f, -79d, HitResult.Ok },
            new object[] { 5f, -79.3d, HitResult.Ok },
            new object[] { 5f, -79.7d, HitResult.Miss },
            new object[] { 5f, -80d, HitResult.Miss },
            new object[] { 5f, -80.2d, HitResult.Miss },
            new object[] { 5f, -80.8d, HitResult.Miss },
            new object[] { 5f, -81d, HitResult.Miss },

            // OD = 7.8 test cases.
            // GREAT hit window is [-25.5ms, 25.5ms]
            // OK    hit window is [-62.5ms, 62.5ms]
            // MISS  hit window is [-80.5ms, 80.5ms]
            new object[] { 7.8f, -25d, HitResult.Great },
            new object[] { 7.8f, -25.4d, HitResult.Great },
            new object[] { 7.8f, -25.8d, HitResult.Ok },
            new object[] { 7.8f, -26d, HitResult.Ok },
            new object[] { 7.8f, -26.1d, HitResult.Ok },
            new object[] { 7.8f, -62d, HitResult.Ok },
            new object[] { 7.8f, -62.4d, HitResult.Ok },
            new object[] { 7.8f, -62.7d, HitResult.Miss },
            new object[] { 7.8f, -63d, HitResult.Miss },
            new object[] { 7.8f, -63.2d, HitResult.Miss },
        };

        [TestCaseSource(nameof(test_cases))]
        public void TestHitWindowStability(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            const double hit_time = 100;

            var beatmap = new TaikoBeatmap
            {
                HitObjects =
                {
                    new Hit
                    {
                        StartTime = hit_time,
                        Type = HitType.Centre,
                    }
                },
                Difficulty = new BeatmapDifficulty { OverallDifficulty = overallDifficulty },
                BeatmapInfo =
                {
                    Ruleset = new TaikoRuleset().RulesetInfo,
                },
            };

            var replay = new Replay
            {
                Frames =
                {
                    new TaikoReplayFrame(0),
                    new TaikoReplayFrame(hit_time + hitOffset, TaikoAction.LeftCentre),
                    new TaikoReplayFrame(hit_time + hitOffset + 20),
                }
            };

            RunTest(beatmap, replay, [expectedResult]);
        }
    }
}
