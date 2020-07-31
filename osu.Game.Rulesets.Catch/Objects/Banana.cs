// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class Banana : Fruit
    {
        /// <summary>
        /// Index of banana in current shower.
        /// </summary>
        public int BananaIndex;

        public override FruitVisualRepresentation VisualRepresentation => FruitVisualRepresentation.Banana;

        public override Judgement CreateJudgement() => new CatchBananaJudgement();

        private static readonly List<HitSampleInfo> samples = new List<HitSampleInfo> { new BananaHitSampleInfo() };

        public Banana()
        {
            Samples = samples;
        }

        private class BananaHitSampleInfo : HitSampleInfo
        {
            private static string[] lookupNames { get; } = { "metronomelow", "catch-banana" };

            public override IEnumerable<string> LookupNames => lookupNames;
        }
    }
}
