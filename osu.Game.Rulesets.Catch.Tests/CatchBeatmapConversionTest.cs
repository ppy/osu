// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Catch.Tests
{
    internal class CatchBeatmapConversionTest : BeatmapConversionTest<TestCatchRuleset, ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Catch";

        [TestCase("basic"), Ignore("See: https://github.com/ppy/osu/issues/2232")]
        public new void Test(string name)
        {
            base.Test(name);
        }

        protected override IEnumerable<ConvertValue> CreateConvertValue(HitObject hitObject)
        {
            if (hitObject is JuiceStream stream)
            {
                foreach (var nested in stream.NestedHitObjects)
                {
                    yield return new ConvertValue
                    {
                        StartTime = nested.StartTime,
                        Position = ((CatchHitObject)nested).X * CatchPlayfield.BASE_WIDTH
                    };
                }
            }
            else
            {
                yield return new ConvertValue
                {
                    StartTime = hitObject.StartTime,
                    Position = ((CatchHitObject)hitObject).X * CatchPlayfield.BASE_WIDTH
                };
            }
        }

        protected override IBeatmapConverter CreateConverter(IBeatmap beatmap) => new CatchBeatmapConverter(beatmap);
    }

    internal struct ConvertValue : IEquatable<ConvertValue>
    {
        /// <summary>
        /// A sane value to account for osu!stable using ints everwhere.
        /// </summary>
        private const float conversion_lenience = 2;

        public double StartTime;
        public float Position;

        public bool Equals(ConvertValue other)
            => Precision.AlmostEquals(StartTime, other.StartTime, conversion_lenience)
               && Precision.AlmostEquals(Position, other.Position, conversion_lenience);
    }

    internal class TestCatchRuleset : CatchRuleset
    {
    }
}
