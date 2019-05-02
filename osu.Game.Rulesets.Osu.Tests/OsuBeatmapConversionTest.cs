// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class OsuBeatmapConversionTest : BeatmapConversionTest<ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Osu";

        [TestCase("basic")]
        [TestCase("colinear-perfect-curve")]
        [TestCase("slider-ticks")]
        public new void Test(string name)
        {
            base.Test(name);
        }

        protected override IEnumerable<ConvertValue> CreateConvertValue(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Slider slider:
                    foreach (var nested in slider.NestedHitObjects)
                        yield return createConvertValue(nested);

                    break;
                default:
                    yield return createConvertValue(hitObject);

                    break;
            }

            ConvertValue createConvertValue(HitObject obj) => new ConvertValue
            {
                StartTime = obj.StartTime,
                EndTime = (obj as IHasEndTime)?.EndTime ?? obj.StartTime,
                X = (obj as IHasPosition)?.X ?? OsuPlayfield.BASE_SIZE.X / 2,
                Y = (obj as IHasPosition)?.Y ?? OsuPlayfield.BASE_SIZE.Y / 2,
            };
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
