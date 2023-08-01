// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Scoring;
using osu.Game.Tests.Beatmaps;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class ManiaUnstableRateEstimationTest
    {
        private const string resource_namespace = "Testing.Beatmaps";
        protected string ResourceAssembly => "osu.Game.Rulesets.Mania";

        // Test that both SS scores and near 0% scores are handled properly, within a margin of +-0.001 UR
        [TestCase(42.978515625d, new[] { 11847, 0, 0, 0, 0, 0 }, "ur-estimation-test")]
        [TestCase(9523485.0d, new[] { 0, 0, 0, 0, 1, 11846 }, "ur-estimation-test")]
        public void Test1(double expectedEstimatedUnstableRate, int[] judgements, string name)
            => TestUnstableRate(expectedEstimatedUnstableRate, judgements, name);

        // General test to make sure UR estimation isn't changed by anything, inclusive of rate changing, within a margin of +-0.001 UR.
        [TestCase(309.990234375d, new[] { 5336, 3886, 1661, 445, 226, 293 }, "ur-estimation-test")]
        public void Test1ClockRateAdjusted(double expectedEstimatedUnstableRate, int[] judgements, string name)
            => TestUnstableRate(expectedEstimatedUnstableRate, judgements, name, new ManiaModDoubleTime());

        // Ensure the UR estimation only returns null when it is supposed to.
        [TestCase(false, new[] { 1, 0, 0, 0, 0, 0 })]
        [TestCase(true, new[] { 0, 0, 0, 0, 0, 1 })]
        [TestCase(true, new[] { 0, 0, 0, 0, 0, 0 })]
        public void Test2(bool returnsNull, int[] judgements)
            => TestNullUnstableRate(returnsNull, judgements);

        // Ensure the estimated deviation doesn't reach too high of a value in a single note situation, as a sanity check.
        [TestCase(new[] { 0, 0, 0, 0, 1, 0 })]
        public void Test3(int[] judgements)
            => TestSingleNoteBound(judgements);

        // Evaluates the Unstable Rate estimation of a beatmap with the given judgements.
        private double? computeUnstableRate(DifficultyAttributes attr, int[] judgementCounts,
                                            [CanBeNull] string resourceName = null, params Mod[] mods)
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

            var beatmapInfo = resourceName == null ? null : getBeatmap(resourceName).BeatmapInfo;
            ManiaPerformanceAttributes perfAttributes = new ManiaPerformanceCalculator().Calculate(
                new ScoreInfo(beatmapInfo)
                {
                    Mods = mods,
                    Statistics = judgements
                }, attr
            );

            return perfAttributes.EstimatedUr;
        }

        protected void TestUnstableRate(double expectedEstimatedUnstableRate, int[] judgementCounts, string name, params Mod[] mods)
        {
            DifficultyAttributes attributes = new ManiaDifficultyCalculator(new ManiaRuleset().RulesetInfo, getBeatmap(name)).Calculate(mods);

            double? estimatedUr = computeUnstableRate(attributes, judgementCounts, name, mods);
            // Platform-dependent math functions (Pow, Cbrt, Exp, etc) and advanced math functions (Erf, FindMinimum) may result in slight differences.
            Assert.That(estimatedUr, Is.EqualTo(expectedEstimatedUnstableRate).Within(0.001), "The estimated mania UR differed from the expected value.");
        }

        protected void TestNullUnstableRate(bool expectedNullStatus, int[] judgementCounts)
        {
            DifficultyAttributes attributes = new ManiaDifficultyAttributes { NoteCount = 1, OverallDifficulty = 10 };

            double? estimatedUr = computeUnstableRate(attributes, judgementCounts);
            bool isNull = estimatedUr == null;

            // Platform-dependent math functions (Pow, Cbrt, Exp, etc) and advanced math functions (Erf, FindMinimum) may result in slight differences.
            Assert.That(isNull, Is.EqualTo(expectedNullStatus), "The estimated mania UR was/wasn't null.");
        }

        protected void TestSingleNoteBound(int[] judgementCounts)
        {
            DifficultyAttributes attributes = new ManiaDifficultyAttributes { NoteCount = 1, OverallDifficulty = 0 };

            double? estimatedUr = computeUnstableRate(attributes, judgementCounts);
            Assert.That(estimatedUr, Is.AtMost(10000.0), "The estimated mania UR returned too high for a single note.");
        }

        protected void TestHitWindows(double overallDifficulty)
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

        private WorkingBeatmap getBeatmap(string name)
        {
            using (var resStream = openResource($"{resource_namespace}.{name}.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoder = Decoder.GetDecoder<Beatmap>(stream);

                ((LegacyBeatmapDecoder)decoder).ApplyOffsets = false;

                return new TestWorkingBeatmap(decoder.Decode(stream))
                {
                    BeatmapInfo =
                    {
                        Ruleset = new ManiaRuleset().RulesetInfo
                    }
                };
            }
        }

        private Stream openResource(string name)
        {
            string localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).AsNonNull();
            return Assembly.LoadFrom(Path.Combine(localPath, $"{ResourceAssembly}.dll")).GetManifestResourceStream($@"{ResourceAssembly}.Resources.{name}");
        }
    }
}
