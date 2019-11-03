// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TaikoBeatmapConversionTest : BeatmapConversionTest<ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Taiko";

        [NonParallelizable]
        [TestCase("basic")]
        [TestCase("slider-generating-drumroll")]
        public void Test(string name) => base.Test(name);

        protected override IEnumerable<ConvertValue> CreateConvertValue(HitObject hitObject)
        {
            yield return new ConvertValue
            {
                StartTime = hitObject.StartTime,
                EndTime = (hitObject as IHasEndTime)?.EndTime ?? hitObject.StartTime,
                IsRim = hitObject is RimHit,
                IsCentre = hitObject is CentreHit,
                IsDrumRoll = hitObject is DrumRoll,
                IsSwell = hitObject is Swell,
                IsStrong = ((TaikoHitObject)hitObject).IsStrong
            };
        }

        protected override Ruleset CreateRuleset() => new TaikoRuleset();
    }

    public struct ConvertValue : IEquatable<ConvertValue>
    {
        /// <summary>
        /// A sane value to account for osu!stable using ints everwhere.
        /// </summary>
        private const float conversion_lenience = 2;

        public double StartTime;
        public double EndTime;
        public bool IsRim;
        public bool IsCentre;
        public bool IsDrumRoll;
        public bool IsSwell;
        public bool IsStrong;

        public bool Equals(ConvertValue other)
            => Precision.AlmostEquals(StartTime, other.StartTime, conversion_lenience)
               && Precision.AlmostEquals(EndTime, other.EndTime, conversion_lenience)
               && IsRim == other.IsRim
               && IsCentre == other.IsCentre
               && IsDrumRoll == other.IsDrumRoll
               && IsSwell == other.IsSwell
               && IsStrong == other.IsStrong;
    }
}
