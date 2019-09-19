// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
    [TestFixture]
    public class ManiaBeatmapConversionTest : BeatmapConversionTest<ManiaConvertMapping, ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Mania";

        [TestCase("basic")]
        public void Test(string name) => base.Test(name);

        protected override IEnumerable<ConvertValue> CreateConvertValue(HitObject hitObject)
        {
            yield return new ConvertValue
            {
                StartTime = hitObject.StartTime,
                EndTime = (hitObject as IHasEndTime)?.EndTime ?? hitObject.StartTime,
                Column = ((ManiaHitObject)hitObject).Column
            };
        }

        private readonly Dictionary<HitObject, RngSnapshot> rngSnapshots = new Dictionary<HitObject, RngSnapshot>();

        protected override void OnConversionGenerated(HitObject original, IEnumerable<HitObject> result, IBeatmapConverter beatmapConverter)
        {
            base.OnConversionGenerated(original, result, beatmapConverter);

            rngSnapshots[original] = new RngSnapshot(beatmapConverter);
        }

        protected override ManiaConvertMapping CreateConvertMapping(HitObject source) => new ManiaConvertMapping(rngSnapshots[source]);

        protected override Ruleset CreateRuleset() => new ManiaRuleset();
    }

    public class RngSnapshot
    {
        public readonly uint RandomW;
        public readonly uint RandomX;
        public readonly uint RandomY;
        public readonly uint RandomZ;

        public RngSnapshot(IBeatmapConverter converter)
        {
            var maniaConverter = (ManiaBeatmapConverter)converter;
            RandomW = maniaConverter.Random.W;
            RandomX = maniaConverter.Random.X;
            RandomY = maniaConverter.Random.Y;
            RandomZ = maniaConverter.Random.Z;
        }
    }

    public class ManiaConvertMapping : ConvertMapping<ConvertValue>, IEquatable<ManiaConvertMapping>
    {
        public uint RandomW;
        public uint RandomX;
        public uint RandomY;
        public uint RandomZ;

        public ManiaConvertMapping()
        {
        }

        public ManiaConvertMapping(RngSnapshot snapshot)
        {
            RandomW = snapshot.RandomW;
            RandomX = snapshot.RandomX;
            RandomY = snapshot.RandomY;
            RandomZ = snapshot.RandomZ;
        }

        public bool Equals(ManiaConvertMapping other) => other != null && RandomW == other.RandomW && RandomX == other.RandomX && RandomY == other.RandomY && RandomZ == other.RandomZ;
        public override bool Equals(ConvertMapping<ConvertValue> other) => base.Equals(other) && Equals(other as ManiaConvertMapping);
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
            => Precision.AlmostEquals(StartTime, other.StartTime, conversion_lenience)
               && Precision.AlmostEquals(EndTime, other.EndTime, conversion_lenience)
               && Column == other.Column;
    }
}
