// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
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
            // PERFECT hit window is [ -19.4ms,  19.4ms]
            // GREAT   hit window is [ -49.0ms,  49.0ms]
            // GOOD    hit window is [ -82.0ms,  82.0ms]
            // OK      hit window is [-112.0ms, 112.0ms]
            // MEH     hit window is [-136.0ms, 136.0ms]
            // MISS    hit window is [-173.0ms, 173.0ms]
            new object[] { 5f, -19d, HitResult.Perfect },
            new object[] { 5f, -19.2d, HitResult.Perfect },
            new object[] { 5f, -19.38d, HitResult.Perfect },
            // new object[] { 5f, -19.4d, HitResult.Perfect }, <- in theory this should work, in practice it does not (fails even before encode & rounding due to floating point precision issues)
            new object[] { 5f, -19.44d, HitResult.Great },
            new object[] { 5f, -19.7d, HitResult.Great },
            new object[] { 5f, -20d, HitResult.Great },
            new object[] { 5f, -48d, HitResult.Great },
            new object[] { 5f, -48.4d, HitResult.Great },
            new object[] { 5f, -48.7d, HitResult.Great },
            new object[] { 5f, -49d, HitResult.Great },
            new object[] { 5f, -49.2d, HitResult.Good },
            new object[] { 5f, -49.7d, HitResult.Good },
            new object[] { 5f, -50d, HitResult.Good },
            new object[] { 5f, -81d, HitResult.Good },
            new object[] { 5f, -81.2d, HitResult.Good },
            new object[] { 5f, -81.7d, HitResult.Good },
            new object[] { 5f, -82d, HitResult.Good },
            new object[] { 5f, -82.2d, HitResult.Ok },
            new object[] { 5f, -82.7d, HitResult.Ok },
            new object[] { 5f, -83d, HitResult.Ok },
            new object[] { 5f, -111d, HitResult.Ok },
            new object[] { 5f, -111.2d, HitResult.Ok },
            new object[] { 5f, -111.7d, HitResult.Ok },
            new object[] { 5f, -112d, HitResult.Ok },
            new object[] { 5f, -112.2d, HitResult.Meh },
            new object[] { 5f, -112.7d, HitResult.Meh },
            new object[] { 5f, -113d, HitResult.Meh },
            new object[] { 5f, -135d, HitResult.Meh },
            new object[] { 5f, -135.2d, HitResult.Meh },
            new object[] { 5f, -135.8d, HitResult.Meh },
            new object[] { 5f, -136d, HitResult.Meh },
            new object[] { 5f, -136.2d, HitResult.Miss },
            new object[] { 5f, -136.7d, HitResult.Miss },
            new object[] { 5f, -137d, HitResult.Miss },

            // OD = 9.3 test cases.
            // PERFECT hit window is [ -14.67ms,  14.67ms]
            // GREAT   hit window is [ -36.10ms,  36.10ms]
            // GOOD    hit window is [ -69.10ms,  69.10ms]
            // OK      hit window is [ -99.10ms,  99.10ms]
            // MEH     hit window is [-123.10ms, 123.10ms]
            // MISS    hit window is [-160.10ms, 160.10ms]
            new object[] { 9.3f, 14d, HitResult.Perfect },
            new object[] { 9.3f, 14.2d, HitResult.Perfect },
            new object[] { 9.3f, 14.6d, HitResult.Perfect },
            // new object[] { 9.3f, 14.67d, HitResult.Perfect }, <- in theory this should work, in practice it does not (fails even before encode & rounding due to floating point precision issues)
            new object[] { 9.3f, 14.7d, HitResult.Great },
            new object[] { 9.3f, 15d, HitResult.Great },
            new object[] { 9.3f, 35d, HitResult.Great },
            new object[] { 9.3f, 35.3d, HitResult.Great },
            new object[] { 9.3f, 35.8d, HitResult.Great },
            new object[] { 9.3f, 36.05d, HitResult.Great },
            new object[] { 9.3f, 36.3d, HitResult.Good },
            new object[] { 9.3f, 36.7d, HitResult.Good },
            new object[] { 9.3f, 37d, HitResult.Good },
            new object[] { 9.3f, 68d, HitResult.Good },
            new object[] { 9.3f, 68.4d, HitResult.Good },
            new object[] { 9.3f, 68.9d, HitResult.Good },
            new object[] { 9.3f, 69.07d, HitResult.Good },
            new object[] { 9.3f, 69.25d, HitResult.Ok },
            new object[] { 9.3f, 69.85d, HitResult.Ok },
            new object[] { 9.3f, 70d, HitResult.Ok },
            new object[] { 9.3f, 98d, HitResult.Ok },
            new object[] { 9.3f, 98.3d, HitResult.Ok },
            new object[] { 9.3f, 98.6d, HitResult.Ok },
            new object[] { 9.3f, 99d, HitResult.Ok },
            new object[] { 9.3f, 99.3d, HitResult.Meh },
            new object[] { 9.3f, 99.7d, HitResult.Meh },
            new object[] { 9.3f, 100d, HitResult.Meh },
            new object[] { 9.3f, 122d, HitResult.Meh },
            new object[] { 9.3f, 122.34d, HitResult.Meh },
            new object[] { 9.3f, 122.57d, HitResult.Meh },
            new object[] { 9.3f, 123.04d, HitResult.Meh },
            new object[] { 9.3f, 123.45d, HitResult.Miss },
            new object[] { 9.3f, 123.95d, HitResult.Miss },
            new object[] { 9.3f, 124d, HitResult.Miss },
        };

        [TestCaseSource(nameof(test_cases))]
        public void TestHitWindowStability(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            const double note_time = 100;

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

            RunTest(beatmap, replay, [expectedResult]);
        }
    }
}
