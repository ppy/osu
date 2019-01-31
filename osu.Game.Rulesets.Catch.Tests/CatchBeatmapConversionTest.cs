﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class CatchBeatmapConversionTest : BeatmapConversionTest<ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Catch";

        [TestCase("basic")]
        [TestCase("spinner")]
        [TestCase("spinner-and-circles")]
        [TestCase("slider")]
        public new void Test(string name)
        {
            base.Test(name);
        }

        protected override IEnumerable<ConvertValue> CreateConvertValue(HitObject hitObject)
        {
            switch (hitObject)
            {
                case JuiceStream stream:
                    foreach (var nested in stream.NestedHitObjects)
                        yield return new ConvertValue((CatchHitObject)nested);
                    break;
                case BananaShower shower:
                    foreach (var nested in shower.NestedHitObjects)
                        yield return new ConvertValue((CatchHitObject)nested);
                    break;
                default:
                    yield return new ConvertValue((CatchHitObject)hitObject);
                    break;
            }
        }

        protected override Ruleset CreateRuleset() => new CatchRuleset();
    }

    public struct ConvertValue : IEquatable<ConvertValue>
    {
        /// <summary>
        /// A sane value to account for osu!stable using ints everwhere.
        /// </summary>
        private const float conversion_lenience = 2;

        [JsonIgnore]
        public readonly CatchHitObject HitObject;

        public ConvertValue(CatchHitObject hitObject)
        {
            HitObject = hitObject;
            startTime = 0;
            position = 0;
        }

        private double startTime;

        public double StartTime
        {
            get => HitObject?.StartTime ?? startTime;
            set => startTime = value;
        }

        private float position;

        public float Position
        {
            get => HitObject?.X * CatchPlayfield.BASE_WIDTH ?? position;
            set => position = value;
        }

        public bool Equals(ConvertValue other)
            => Precision.AlmostEquals(StartTime, other.StartTime, conversion_lenience)
               && Precision.AlmostEquals(Position, other.Position, conversion_lenience);
    }
}
