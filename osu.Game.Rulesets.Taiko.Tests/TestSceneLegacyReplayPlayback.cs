// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [Ignore("These tests are expected to fail until an acceptable solution for various replay playback issues concerning rounding of replay frame times & hit windows is found.")]
    public partial class TestSceneLegacyReplayPlayback : LegacyReplayPlaybackTestScene
    {
        protected override string? ExportLocation => null;

        protected override Ruleset CreateRuleset() => new TaikoRuleset();

        private static readonly object[][] test_cases =
        {
            // With respect to notation,
            // square brackets `[]` represent *closed* or *inclusive* bounds,
            // while round brackets `()` represent *open* or *exclusive* bounds.

            // OD = 5 test cases.
            // GREAT hit window is (-35ms, 35ms)
            // OK    hit window is (-80ms, 80ms)
            new object[] { 5f, -33d, HitResult.Great },
            new object[] { 5f, -34d, HitResult.Great },
            new object[] { 5f, -35d, HitResult.Ok },
            new object[] { 5f, -36d, HitResult.Ok },
            new object[] { 5f, -78d, HitResult.Ok },
            new object[] { 5f, -79d, HitResult.Ok },
            new object[] { 5f, -80d, HitResult.Miss },
            new object[] { 5f, -81d, HitResult.Miss },

            // OD = 7.8 test cases.
            // GREAT hit window is (-26ms, 26ms)
            // OK    hit window is (-63ms, 63ms)
            new object[] { 7.8f, -24d, HitResult.Great },
            new object[] { 7.8f, -25d, HitResult.Great },
            new object[] { 7.8f, -26d, HitResult.Ok },
            new object[] { 7.8f, -27d, HitResult.Ok },
            new object[] { 7.8f, -61d, HitResult.Ok },
            new object[] { 7.8f, -62d, HitResult.Ok },
            new object[] { 7.8f, -63d, HitResult.Miss },
            new object[] { 7.8f, -64d, HitResult.Miss },
        };

        [TestCaseSource(nameof(test_cases))]
        public void TestHitWindowTreatment(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            const double hit_time = 100;

            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 1000 });
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
                ControlPointInfo = cpi,
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

            var score = new Score
            {
                Replay = replay,
                ScoreInfo = new ScoreInfo
                {
                    Ruleset = CreateRuleset().RulesetInfo,
                }
            };

            RunTest($@"single hit @ OD{overallDifficulty}", beatmap, $@"{hitOffset}ms @ OD{overallDifficulty} = {expectedResult}", score, [expectedResult]);
        }
    }
}
