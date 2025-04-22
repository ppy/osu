// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.Beatmaps;
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

        private static readonly object[][] test_cases =
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

        [TestCaseSource(nameof(test_cases))]
        public void TestHitWindowTreatment(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            const double hit_circle_time = 100;

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
    }
}
