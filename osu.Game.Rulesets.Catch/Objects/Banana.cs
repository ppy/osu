// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Judgements;
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

        public class BananaHitSampleInfo : HitSampleInfo, IEquatable<BananaHitSampleInfo>
        {
            private static readonly string[] lookup_names = { "Gameplay/metronomelow", "Gameplay/catch-banana" };

            public override IEnumerable<string> LookupNames => lookup_names;

            public BananaHitSampleInfo()
                : this(string.Empty)
            {
            }

            public BananaHitSampleInfo(HitSampleInfo info)
                : this(info.Name, info.Bank, info.Suffix, info.Volume, info.EditorAutoBank, info.UseBeatmapSamples)
            {
            }

            private BananaHitSampleInfo(string name, string bank = SampleControlPoint.DEFAULT_BANK, string? suffix = null, int volume = 100, bool editorAutoBank = true, bool useBeatmapSamples = false)
                : base(name, bank, suffix, volume, editorAutoBank, useBeatmapSamples)
            {
            }

            public sealed override HitSampleInfo With(Optional<string> newName = default, Optional<string> newBank = default, Optional<string?> newSuffix = default, Optional<int> newVolume = default,
                                                      Optional<bool> newEditorAutoBank = default, Optional<bool> newUseBeatmapSamples = default)
                => new BananaHitSampleInfo(newName.GetOr(Name), newBank.GetOr(Bank), newSuffix.GetOr(Suffix), newVolume.GetOr(Volume),
                    newEditorAutoBank.GetOr(EditorAutoBank), newUseBeatmapSamples.GetOr(UseBeatmapSamples));

            public bool Equals(BananaHitSampleInfo? other)
                => other != null;

            public override bool Equals(object? obj)
                => obj is BananaHitSampleInfo other && Equals(other);

            public override int GetHashCode() => lookup_names.GetHashCode();
        }
    }
}
