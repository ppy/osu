// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class Banana : Fruit, IHasComboInformation
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

        private Color4? colour;

        Color4 IHasComboInformation.GetComboColour(IReadOnlyList<Color4> comboColours)
        {
            // override any external colour changes with banananana
            return colour ??= getBananaColour();
        }

        private Color4 getBananaColour()
        {
            switch (RNG.Next(0, 3))
            {
                default:
                    return new Color4(255, 240, 0, 255);

                case 1:
                    return new Color4(255, 192, 0, 255);

                case 2:
                    return new Color4(214, 221, 28, 255);
            }
        }

        private class BananaHitSampleInfo : HitSampleInfo
        {
            private static string[] lookupNames { get; } = { "metronomelow", "catch-banana" };

            public override IEnumerable<string> LookupNames => lookupNames;
        }
    }
}
