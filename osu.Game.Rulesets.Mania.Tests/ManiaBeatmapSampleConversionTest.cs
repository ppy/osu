// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaBeatmapSampleConversionTest : BeatmapConversionTest<ConvertMapping<SampleConvertValue>, SampleConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Mania.Tests";

        [TestCase("convert-samples")]
        [TestCase("mania-samples")]
        [TestCase("slider-convert-samples")]
        public void Test(string name) => base.Test(name);

        protected override IEnumerable<SampleConvertValue> CreateConvertValue(HitObject hitObject)
        {
            yield return new SampleConvertValue
            {
                StartTime = hitObject.StartTime,
                EndTime = hitObject.GetEndTime(),
                Column = ((ManiaHitObject)hitObject).Column,
                Samples = getSampleNames(hitObject.Samples),
                NodeSamples = getNodeSampleNames((hitObject as HoldNote)?.NodeSamples)
            };
        }

        private IList<string> getSampleNames(IList<HitSampleInfo> hitSampleInfo)
            => hitSampleInfo.Select(sample => sample.LookupNames.First()).ToList();

        private IList<IList<string>> getNodeSampleNames(IList<IList<HitSampleInfo>> hitSampleInfo)
            => hitSampleInfo?.Select(getSampleNames)
                            .ToList();

        protected override Ruleset CreateRuleset() => new ManiaRuleset();
    }

    public struct SampleConvertValue : IEquatable<SampleConvertValue>
    {
        /// <summary>
        /// A sane value to account for osu!stable using ints everywhere.
        /// </summary>
        private const float conversion_lenience = 2;

        public double StartTime;
        public double EndTime;
        public int Column;
        public IList<string> Samples;
        public IList<IList<string>> NodeSamples;

        public bool Equals(SampleConvertValue other)
            => Precision.AlmostEquals(StartTime, other.StartTime, conversion_lenience)
               && Precision.AlmostEquals(EndTime, other.EndTime, conversion_lenience)
               && samplesEqual(Samples, other.Samples)
               && nodeSamplesEqual(NodeSamples, other.NodeSamples);

        private static bool samplesEqual(ICollection<string> firstSampleList, ICollection<string> secondSampleList)
            => firstSampleList.SequenceEqual(secondSampleList);

        private static bool nodeSamplesEqual(ICollection<IList<string>> firstSampleList, ICollection<IList<string>> secondSampleList)
        {
            if (firstSampleList == null && secondSampleList == null)
                return true;

            // both items can't be null now, so if any single one is, then they're not equal
            if (firstSampleList == null || secondSampleList == null)
                return false;

            return firstSampleList.Count == secondSampleList.Count
                   // cannot use .Zip() without the selector function as it doesn't compile in android test project
                   && firstSampleList.Zip(secondSampleList, (first, second) => (first, second))
                                     .All(samples => samples.first.SequenceEqual(samples.second));
        }
    }
}
