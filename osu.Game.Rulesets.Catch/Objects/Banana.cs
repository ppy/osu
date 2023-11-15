// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class Banana : PalpableCatchHitObject, IHasComboInformation
    {
        /// <summary>
        /// Index of banana in current shower.
        /// </summary>
        public int BananaIndex;

        public override Judgement CreateJudgement() => new CatchBananaJudgement();

        private static readonly IList<HitSampleInfo> default_banana_samples = new List<HitSampleInfo> { new BananaHitSampleInfo() }.AsReadOnly();

        public Banana()
        {
            Samples = default_banana_samples;
        }

        // override any external colour changes with banananana
        Color4 IHasComboInformation.GetComboColour(ISkin skin) => getBananaColour();

        private Color4 getBananaColour()
        {
            switch (StatelessRNG.NextInt(3, RandomSeed))
            {
                default:
                    return new Color4(255, 240, 0, 255);

                case 1:
                    return new Color4(255, 192, 0, 255);

                case 2:
                    return new Color4(214, 221, 28, 255);
            }
        }

        protected override void CopyFrom(HitObject other, IDictionary<object, object>? referenceLookup = null)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not Banana banana)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(Banana)}");

            BananaIndex = banana.BananaIndex;
        }

        protected override HitObject CreateInstance() => new Banana();

        public class BananaHitSampleInfo : HitSampleInfo, IEquatable<BananaHitSampleInfo>
        {
            private static readonly string[] lookup_names = { "Gameplay/metronomelow", "Gameplay/catch-banana" };

            public override IEnumerable<string> LookupNames => lookup_names;

            public BananaHitSampleInfo(int volume = 100)
                : base(string.Empty, volume: volume)
            {
            }

            public sealed override HitSampleInfo With(Optional<string> newName = default, Optional<string> newBank = default, Optional<string?> newSuffix = default, Optional<int> newVolume = default)
                => new BananaHitSampleInfo(newVolume.GetOr(Volume));

            public bool Equals(BananaHitSampleInfo? other)
                => other != null;

            public override bool Equals(object? obj)
                => obj is BananaHitSampleInfo other && Equals(other);

            public override int GetHashCode() => lookup_names.GetHashCode();

            protected override HitSampleInfo CreateInstance() => new BananaHitSampleInfo();
        }
    }
}
