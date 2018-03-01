// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Newtonsoft.Json;
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
    public class OsuBeatmapConversionTest : BeatmapConversionTest<ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Osu";

        [TestCase(875945)]
        public new void Test(int beatmapId)
        {
            base.Test(beatmapId);
        }

        protected override ConvertValue CreateConvertValue(HitObject hitObject)
        {
            var startPosition = (hitObject as IHasPosition)?.Position ?? new Vector2(256, 192);
            var endPosition = (hitObject as Slider)?.EndPosition ?? startPosition;

            return new ConvertValue
            {
                StartTime = hitObject.StartTime,
                EndTime = (hitObject as IHasEndTime)?.EndTime ?? hitObject.StartTime,
                StartX = startPosition.X,
                StartY = startPosition.Y,
                EndX = endPosition.X,
                EndY = endPosition.Y
            };
        }

        protected override ITestableBeatmapConverter CreateConverter() => new OsuBeatmapConverter();
    }

    public struct ConvertValue : IEquatable<ConvertValue>
    {
        [JsonProperty]
        public double StartTime;
        [JsonProperty]
        public double EndTime;
        [JsonProperty]
        public float StartX;
        [JsonProperty]
        public float StartY;
        [JsonProperty]
        public float EndX;
        [JsonProperty]
        public float EndY;

        public bool Equals(ConvertValue other)
            => Precision.AlmostEquals(StartTime, other.StartTime, 1)
               && Precision.AlmostEquals(EndTime, other.EndTime, 1)
               && Precision.AlmostEquals(StartX, other.StartX, 1)
               && Precision.AlmostEquals(StartY, other.StartY, 1)
               && Precision.AlmostEquals(EndX, other.EndX, 1)
               && Precision.AlmostEquals(EndY, other.EndY, 1);
    }
}
