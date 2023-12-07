// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class OsuBeatmapConversionTest : BeatmapConversionTest<ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Osu.Tests";

        [TestCase("basic")]
        [TestCase("colinear-perfect-curve")]
        [TestCase("slider-ticks")]
        [TestCase("slider-ticks-edge-case")]
        [TestCase("slider-paths-edge-case")]
        [TestCase("repeat-slider")]
        [TestCase("uneven-repeat-slider")]
        [TestCase("old-stacking")]
        [TestCase("multi-segment-slider")]
        [TestCase("nan-slider")]
        [TestCase("1124896")]
        public void Test(string name) => base.Test(name);

        protected override IEnumerable<ConvertValue> CreateConvertValue(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Slider slider:
                    foreach (var nested in slider.NestedHitObjects)
                        yield return createConvertValue((OsuHitObject)nested, slider);

                    break;

                default:
                    yield return createConvertValue((OsuHitObject)hitObject);

                    break;
            }

            static ConvertValue createConvertValue(OsuHitObject obj, OsuHitObject? parent = null)
            {
                double startTime = obj.StartTime;
                double endTime = obj.GetEndTime();

                // as stated in the inline comment above, this is locally bringing back
                // the stable treatment of the "legacy last tick" just to make sure
                // that the conversion output matches.
                // compare: `SliderEventGenerator.Generate()`, and the calculation of `legacyLastTickTime`.
                if (obj is SliderTailCircle && parent is Slider slider)
                {
                    startTime = Math.Max(startTime + SliderEventGenerator.TAIL_LENIENCY, slider.StartTime + slider.Duration / 2);
                    endTime = Math.Max(endTime + SliderEventGenerator.TAIL_LENIENCY, slider.StartTime + slider.Duration / 2);
                }

                return new ConvertValue
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    X = obj.StackedPosition.X,
                    Y = obj.StackedPosition.Y
                };
            }
        }

        protected override Ruleset CreateRuleset() => new OsuRuleset();
    }

    public struct ConvertValue : IEquatable<ConvertValue>
    {
        /// <summary>
        /// A sane value to account for osu!stable using <see cref="int"/>s everywhere.
        /// </summary>
        private const double conversion_lenience = 2;

        public double StartTime;
        public double EndTime;
        public float X;
        public float Y;

        public bool Equals(ConvertValue other)
            => Precision.AlmostEquals(StartTime, other.StartTime, conversion_lenience)
               && Precision.AlmostEquals(EndTime, other.EndTime, conversion_lenience)
               && Precision.AlmostEquals(X, other.X, conversion_lenience)
               && Precision.AlmostEquals(Y, other.Y, conversion_lenience);
    }
}
