// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class OsuBeatmapConversionTest : BeatmapConversionTest<TestOsuRuleset, ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Osu";

        [TestCase("basic")]
        [TestCase("colinear-perfect-curve")]
        public new void Test(string name)
        {
            base.Test(name);
        }

        protected override IEnumerable<ConvertValue> CreateConvertValue(HitObject hitObject)
        {
            var startPosition = (hitObject as IHasPosition)?.Position ?? new Vector2(256, 192);
            var endPosition = (hitObject as Slider)?.EndPosition ?? startPosition;

            yield return new ConvertValue
            {
                StartTime = hitObject.StartTime,
                EndTime = (hitObject as IHasEndTime)?.EndTime ?? hitObject.StartTime,
                StartX = startPosition.X,
                StartY = startPosition.Y,
                EndX = endPosition.X,
                EndY = endPosition.Y
            };
        }

        protected override IBeatmapConverter CreateConverter(IBeatmap beatmap) => new OsuBeatmapConverter(beatmap);
    }

    public struct ConvertValue : IEquatable<ConvertValue>
    {
        /// <summary>
        /// A sane value to account for osu!stable using ints everwhere.
        /// </summary>
        private const double conversion_lenience = 2;

        public double StartTime;
        public double EndTime;
        public float StartX;
        public float StartY;
        public float EndX;
        public float EndY;

        public bool Equals(ConvertValue other)
            => Precision.AlmostEquals(StartTime, other.StartTime)
               && Precision.AlmostEquals(EndTime, other.EndTime, conversion_lenience)
               && Precision.AlmostEquals(StartX, other.StartX)
               && Precision.AlmostEquals(StartY, other.StartY, conversion_lenience)
               && Precision.AlmostEquals(EndX, other.EndX, conversion_lenience)
               && Precision.AlmostEquals(EndY, other.EndY, conversion_lenience);
    }

    public class TestOsuRuleset : OsuRuleset
    {
    }
}
