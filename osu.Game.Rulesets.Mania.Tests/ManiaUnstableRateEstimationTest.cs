// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Scoring;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Tests
{
    /// <summary>
    /// This test suite tests ManiaPerformanceCalculator.computeEstimatedUr
    /// <remarks>
    /// This suite focuses on the objective aspects of the calculation, not the accuracy of the calculation.
    /// </remarks>
    /// </summary>
    public class ManiaUnstableRateEstimationTest
    {
        public enum SpeedMod
        {
            DoubleTime,
            NormalTime,
            HalfTime
        }

        public static IEnumerable<TestCaseData> TestCaseSourceData()
        {
            yield return new TestCaseData(691.640625d, new[] { 3, 3, 3, 3, 3, 3 }, SpeedMod.DoubleTime);
            yield return new TestCaseData(1037.4609375d, new[] { 3, 3, 3, 3, 3, 3 }, SpeedMod.NormalTime);
            yield return new TestCaseData(1383.28125d, new[] { 3, 3, 3, 3, 3, 3 }, SpeedMod.HalfTime);
        }

        /// <summary>
        /// A catch-all hardcoded regression test, inclusive of rate changing.
        /// </summary>
        [TestCaseSource(nameof(TestCaseSourceData))]
        public void RegressionTest(double expectedUr, int[] judgementCounts, SpeedMod speedMod)
        {
            double? estimatedUr = computeUnstableRate(judgementCounts, speedMod: speedMod);
            // Platform-dependent math functions (Pow, Cbrt, Exp, etc) and advanced math functions (Erf, FindMinimum) may result in slight differences.
            Assert.That(
                estimatedUr, Is.EqualTo(expectedUr).Within(0.001),
                $"The estimated mania UR {estimatedUr} differed from the expected value {expectedUr}."
            );
        }

        /// <summary>
        /// Test anomalous judgement counts where NULLs can occur.
        /// </summary>
        [TestCase(false, new[] { 1, 0, 0, 0, 0, 0 })]
        [TestCase(true, new[] { 0, 0, 0, 0, 0, 1 })]
        [TestCase(true, new[] { 0, 0, 0, 0, 0, 0 })]
        public void TestNull(bool expectedIsNull, int[] judgementCounts)
        {
            double? estimatedUr = computeUnstableRate(judgementCounts);
            bool isNull = estimatedUr == null;

            Assert.That(isNull, Is.EqualTo(expectedIsNull), $"Estimated mania UR {estimatedUr} was/wasn't null.");
        }

        /// <summary>
        /// Ensure that the worst case scenarios don't result in unbounded URs.
        /// <remarks>Given Int.MaxValue judgements, it can result in
        /// <see cref="MathNet.Numerics.Optimization.MaximumIterationsException"/>.
        /// However, we'll only test realistic scenarios.</remarks>
        /// </summary>
        [Test, Combinatorial]
        public void TestEdge(
            [Values(100_000, 1, 0)] int judgeMax, // We're only interested in the edge judgements.
            [Values(100_000, 1, 0)] int judge50,
            [Values(100_000, 1, 0)] int judge0,
            [Values(SpeedMod.DoubleTime, SpeedMod.HalfTime, SpeedMod.NormalTime)]
            SpeedMod speedMod,
            [Values(true, false)] bool isHoldsLegacy,
            [Values(true, false)] bool isAllHolds, // This will determine if we use all holds or all notes.
            [Values(10, 5, 0)] double od
        )
        {
            // This is tested in TestNull.
            if (judgeMax + judge50 == 0) Assert.Ignore();

            int noteCount = isAllHolds ? 0 : judgeMax + judge50 + judge0;
            int holdCount = isAllHolds ? judgeMax + judge50 + judge0 : 0;

            double? estimatedUr = computeUnstableRate(
                new[] { judgeMax, 0, 0, 0, judge50, judge0 },
                noteCount,
                holdCount,
                od,
                speedMod,
                isHoldsLegacy
            );
            Assert.That(
                estimatedUr, Is.AtMost(1_000_000_000),
                $"The estimated mania UR {estimatedUr} returned too high for a single note."
            );
        }

        /// <summary>
        /// This tests if the UR gets smaller, given more judgements on MAX.
        /// This follows the logic that:
        ///   - More MAX judgements implies stronger evidence of smaller UR, as the probability of hitting more MAX is lower.
        /// <remarks>
        /// It's not necessary, nor logical to test other behaviors.
        /// </remarks>
        /// </summary>
        [Test]
        public void TestMoreMaxJudgementsSmallerUr(
            [Values(1, 10, 1000)] int count,
            [Values(1, 10, 1000)] int step
        )
        {
            int[] judgementCountsLess = { count, 0, 0, 0, 0, 0 };
            int[] judgementCountsMore = { count + step, 0, 0, 0, 0, 0 };
            double? estimatedUrLessJudgements = computeUnstableRate(judgementCountsLess);
            double? estimatedUrMoreJudgements = computeUnstableRate(judgementCountsMore);

            // Assert that More Judgements results in a smaller UR.
            Assert.That(
                estimatedUrMoreJudgements, Is.LessThan(estimatedUrLessJudgements),
                $"UR {estimatedUrMoreJudgements} with More Judgements {string.Join(",", judgementCountsMore)} >= "
                + $"UR {estimatedUrLessJudgements} than Less Judgements {string.Join(",", judgementCountsLess)} "
            );
        }

        /// <summary>
        /// Evaluates the Unstable Rate
        /// </summary>
        /// <param name="judgementCounts">Size-6 Int List of Judgements, starting from MAX</param>
        /// <param name="noteCount">Number of notes</param>
        /// <param name="holdCount">Number of holds</param>
        /// <param name="od">Overall Difficulty</param>
        /// <param name="speedMod">Speed Mod, <see cref="SpeedMod"/></param>
        /// <param name="isHoldsLegacy">Whether to append ClassicMod to simulate Legacy Holds</param>
        private double? computeUnstableRate(
            IReadOnlyList<int> judgementCounts,
            int? noteCount = null,
            int holdCount = 0,
            double od = 5,
            SpeedMod speedMod = SpeedMod.NormalTime,
            bool isHoldsLegacy = false)
        {
            var judgements = new Dictionary<HitResult, int>
            {
                { HitResult.Perfect, judgementCounts[0] },
                { HitResult.Great, judgementCounts[1] },
                { HitResult.Good, judgementCounts[2] },
                { HitResult.Ok, judgementCounts[3] },
                { HitResult.Meh, judgementCounts[4] },
                { HitResult.Miss, judgementCounts[5] }
            };
            noteCount ??= judgements.Sum(kvp => kvp.Value);

            var mods = new Mod[] { };

            if (isHoldsLegacy) mods = mods.Append(new ManiaModClassic()).ToArray();

            switch (speedMod)
            {
                case SpeedMod.DoubleTime:
                    mods = mods.Append(new ManiaModDoubleTime()).ToArray();
                    break;

                case SpeedMod.HalfTime:
                    mods = mods.Append(new ManiaModHalfTime()).ToArray();
                    break;
            }

            ManiaPerformanceAttributes perfAttributes = new ManiaPerformanceCalculator().Calculate(
                new ScoreInfo
                {
                    Mods = mods,
                    Statistics = judgements
                },
                new ManiaDifficultyAttributes
                {
                    NoteCount = (int)noteCount,
                    HoldNoteCount = holdCount,
                    OverallDifficulty = od,
                    Mods = mods
                }
            );

            return perfAttributes.EstimatedUr;
        }

        /// <summary>
        /// This ensures that external changes of hit windows don't break the ur calculator.
        /// This includes all ODs.
        /// </summary>
        [Test]
        public void RegressionTestHitWindows(
            [Range(0, 10, 0.5)] double overallDifficulty
        )
        {
            DifficultyAttributes attributes = new ManiaDifficultyAttributes { OverallDifficulty = overallDifficulty };

            var hitWindows = new ManiaHitWindows();
            hitWindows.SetDifficulty(overallDifficulty);

            double[] trueHitWindows =
            {
                hitWindows.WindowFor(HitResult.Perfect),
                hitWindows.WindowFor(HitResult.Great),
                hitWindows.WindowFor(HitResult.Good),
                hitWindows.WindowFor(HitResult.Ok),
                hitWindows.WindowFor(HitResult.Meh)
            };

            ManiaPerformanceAttributes perfAttributes = new ManiaPerformanceCalculator().Calculate(new ScoreInfo(), attributes);

            // Platform-dependent math functions (Pow, Cbrt, Exp, etc) may result in minute differences.
            Assert.That(perfAttributes.HitWindows, Is.EqualTo(trueHitWindows).Within(0.000001), "The true mania hit windows are different to the ones calculated in ManiaPerformanceCalculator.");
        }
    }
}
