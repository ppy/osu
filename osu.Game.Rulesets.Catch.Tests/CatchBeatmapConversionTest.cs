// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class CatchBeatmapConversionTest : BeatmapConversionTest<ConvertValue>
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Catch.Tests";

        [TestCase("basic")]
        [TestCase("spinner")]
        [TestCase("spinner-and-circles")]
        [TestCase("slider")]
        [TestCase("hardrock-stream", new[] { typeof(CatchModHardRock) })]
        [TestCase("hardrock-repeat-slider", new[] { typeof(CatchModHardRock) })]
        [TestCase("hardrock-spinner", new[] { typeof(CatchModHardRock) })]
        [TestCase("right-bound-hr-offset", new[] { typeof(CatchModHardRock) })]
        [TestCase("basic-hyperdash")]
        [TestCase("pixel-jump")]
        [TestCase("tiny-ticks")]
        [TestCase("v8-tick-distance")]
        [TestCase("spinner-precision")]
        [TestCase("37902", new[] { typeof(CatchModDoubleTime), typeof(CatchModHardRock), typeof(CatchModHidden) })]
        [TestCase("39206", new[] { typeof(CatchModDoubleTime), typeof(CatchModHidden) })]
        [TestCase("42587")]
        [TestCase("50859", new[] { typeof(CatchModDoubleTime), typeof(CatchModHidden) })]
        [TestCase("75858", new[] { typeof(CatchModHardRock), typeof(CatchModHidden) })]
        [TestCase("103019", new[] { typeof(CatchModHidden) })]
        [TestCase("104973", new[] { typeof(CatchModHardRock), typeof(CatchModHidden) })]
        [TestCase("871815", new[] { typeof(CatchModDoubleTime), typeof(CatchModHidden) })]
        [TestCase("1284935", new[] { typeof(CatchModDoubleTime), typeof(CatchModHardRock) })]
        [TestCase("1431386", new[] { typeof(CatchModDoubleTime), typeof(CatchModHardRock), typeof(CatchModHidden) })]
        [TestCase("1597806", new[] { typeof(CatchModDoubleTime), typeof(CatchModHidden) })]
        [TestCase("2190499", new[] { typeof(CatchModDoubleTime), typeof(CatchModHidden) })]
        [TestCase("2571731", new[] { typeof(CatchModHardRock), typeof(CatchModHidden) })]
        [TestCase("2768615", new[] { typeof(CatchModDoubleTime), typeof(CatchModHardRock) })]
        [TestCase("2781126", new[] { typeof(CatchModHidden) })]
        [TestCase("3152510", new[] { typeof(CatchModDoubleTime) })]
        [TestCase("3227428", new[] { typeof(CatchModHardRock), typeof(CatchModHidden) })]
        [TestCase("3524302", new[] { typeof(CatchModDoubleTime), typeof(CatchModEasy) })]
        [TestCase("3644427", new[] { typeof(CatchModEasy), typeof(CatchModFlashlight) })]
        [TestCase("3689906", new[] { typeof(CatchModDoubleTime), typeof(CatchModEasy) })]
        [TestCase("3949367", new[] { typeof(CatchModDoubleTime), typeof(CatchModEasy) })]
        public new void Test(string name, params Type[] mods) => base.Test(name, mods);

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
        private const float conversion_lenience = 3;

        [JsonIgnore]
        public readonly CatchHitObject HitObject;

        public ConvertValue(CatchHitObject hitObject)
        {
            HitObject = hitObject;
            startTime = 0;
            position = 0;
            hyperDash = false;
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
            get => HitObject?.EffectiveX ?? position;
            set => position = value;
        }

        private bool hyperDash;

        public bool HyperDash
        {
            get => (HitObject as PalpableCatchHitObject)?.HyperDash ?? hyperDash;
            set => hyperDash = value;
        }

        public bool Equals(ConvertValue other)
            => Precision.AlmostEquals(StartTime, other.StartTime, conversion_lenience)
               && Precision.AlmostEquals(Position, other.Position, conversion_lenience)
               && HyperDash == other.HyperDash;
    }
}
