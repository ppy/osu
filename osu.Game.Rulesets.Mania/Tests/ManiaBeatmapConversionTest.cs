// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using NUnit.Framework;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class ManiaBeatmapConversionTest : BeatmapConversionTest<ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Mania";

        private bool isForCurrentRuleset;

        [TestCase(875945, false), NonParallelizable]
        public void Test(int beatmapId, bool isForCurrentRuleset)
        {
            this.isForCurrentRuleset = isForCurrentRuleset;
            base.Test(beatmapId);
        }

        protected override ConvertValue CreateConvertValue(HitObject hitObject) => new ConvertValue
        {
            StartTime = hitObject.StartTime,
            EndTime = (hitObject as IHasEndTime)?.EndTime ?? hitObject.StartTime,
            Column = ((ManiaHitObject)hitObject).Column
        };

        protected override ITestableBeatmapConverter CreateConverter(Beatmap beatmap) => new ManiaBeatmapConverter(isForCurrentRuleset, beatmap);
    }

    public struct ConvertValue : IEquatable<ConvertValue>
    {
        /// <summary>
        /// A sane value to account for osu!stable using ints everwhere.
        /// </summary>
        private const float conversion_lenience = 2;

        public double StartTime;
        public double EndTime;
        public int Column;

        public bool Equals(ConvertValue other)
            => Precision.AlmostEquals(StartTime, other.StartTime)
               && Precision.AlmostEquals(EndTime, other.EndTime, conversion_lenience)
               && Column == other.Column;
    }
}
