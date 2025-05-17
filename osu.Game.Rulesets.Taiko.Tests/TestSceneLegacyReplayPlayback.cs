// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneLegacyReplayPlayback : LegacyReplayPlaybackTestScene
    {
        protected override string? ExportLocation => null;

        protected override Ruleset CreateRuleset() => new TaikoRuleset();

        private static readonly object[][] no_mod_test_cases =
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

        private static readonly object[][] hard_rock_test_cases =
        {
            // OD = 5 test cases.
            // This leads to "effective" OD of 7.
            // GREAT hit window is (-29ms, 29ms)
            // OK    hit window is (-68ms, 68ms)
            new object[] { 5f, -27d, HitResult.Great },
            new object[] { 5f, -28d, HitResult.Great },
            new object[] { 5f, -29d, HitResult.Ok },
            new object[] { 5f, -30d, HitResult.Ok },
            new object[] { 5f, -66d, HitResult.Ok },
            new object[] { 5f, -67d, HitResult.Ok },
            new object[] { 5f, -68d, HitResult.Miss },
            new object[] { 5f, -69d, HitResult.Miss },

            // OD = 7.8 test cases.
            // This would lead to "effective" OD of 10.92,
            // but the effects are capped to OD 10.
            // GREAT hit window is (-20ms, 20ms)
            // OK    hit window is (-50ms, 50ms)
            new object[] { 7.8f, -18d, HitResult.Great },
            new object[] { 7.8f, -19d, HitResult.Great },
            new object[] { 7.8f, -20d, HitResult.Ok },
            new object[] { 7.8f, -21d, HitResult.Ok },
            new object[] { 7.8f, -48d, HitResult.Ok },
            new object[] { 7.8f, -49d, HitResult.Ok },
            new object[] { 7.8f, -50d, HitResult.Miss },
            new object[] { 7.8f, -51d, HitResult.Miss },
        };

        private static readonly object[][] easy_test_cases =
        {
            // OD = 5 test cases.
            // This leads to "effective" OD of 2.5.
            // GREAT hit window is ( -42ms,  42ms)
            // OK    hit window is (-100ms, 100ms)
            new object[] { 5f, -40d, HitResult.Great },
            new object[] { 5f, -41d, HitResult.Great },
            new object[] { 5f, -42d, HitResult.Ok },
            new object[] { 5f, -43d, HitResult.Ok },
            new object[] { 5f, -98d, HitResult.Ok },
            new object[] { 5f, -99d, HitResult.Ok },
            new object[] { 5f, -100d, HitResult.Miss },
            new object[] { 5f, -101d, HitResult.Miss },
        };

        private const double hit_time = 100;

        [TestCaseSource(nameof(no_mod_test_cases))]
        public void TestHitWindowTreatment(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            var beatmap = createBeatmap(overallDifficulty);

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

        [TestCaseSource(nameof(hard_rock_test_cases))]
        public void TestHitWindowTreatmentWithHardRock(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            var beatmap = createBeatmap(overallDifficulty);

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
                    Mods = [new TaikoModHardRock()]
                }
            };

            RunTest($@"HR single hit @ OD{overallDifficulty}", beatmap, $@"HR {hitOffset}ms @ OD{overallDifficulty} = {expectedResult}", score, [expectedResult]);
        }

        [TestCaseSource(nameof(easy_test_cases))]
        public void TestHitWindowTreatmentWithEasy(float overallDifficulty, double hitOffset, HitResult expectedResult)
        {
            var beatmap = createBeatmap(overallDifficulty);

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
                    Mods = [new TaikoModEasy()]
                }
            };

            RunTest($@"EZ single hit @ OD{overallDifficulty}", beatmap, $@"EZ {hitOffset}ms @ OD{overallDifficulty} = {expectedResult}", score, [expectedResult]);
        }

        private static TaikoBeatmap createBeatmap(float overallDifficulty)
        {
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
            return beatmap;
        }
    }
}
