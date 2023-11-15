// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Hit : TaikoStrongableHitObject, IHasDisplayColour
    {
        private HitObjectProperty<HitType> type;

        public Bindable<HitType> TypeBindable => type.Bindable;

        /// <summary>
        /// The <see cref="HitType"/> that actuates this <see cref="Hit"/>.
        /// </summary>
        public HitType Type
        {
            get => type.Value;
            set => type.Value = value;
        }

        public Bindable<Color4> DisplayColour { get; } = new Bindable<Color4>(COLOUR_CENTRE);

        public static readonly Color4 COLOUR_CENTRE = Color4Extensions.FromHex(@"bb1177");
        public static readonly Color4 COLOUR_RIM = Color4Extensions.FromHex(@"2299bb");

        public Hit()
        {
            TypeBindable.BindValueChanged(_ =>
            {
                updateSamplesFromType();
                DisplayColour.Value = Type == HitType.Centre ? COLOUR_CENTRE : COLOUR_RIM;
            });

            SamplesBindable.BindCollectionChanged((_, _) => updateTypeFromSamples());
        }

        private void updateTypeFromSamples()
        {
            Type = getRimSamples().Any() ? HitType.Rim : HitType.Centre;
        }

        /// <summary>
        /// Returns an array of any samples which would cause this object to be a "rim" type hit.
        /// </summary>
        private HitSampleInfo[] getRimSamples() => Samples.Where(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE).ToArray();

        private void updateSamplesFromType()
        {
            var rimSamples = getRimSamples();

            bool isRimType = Type == HitType.Rim;

            if (isRimType != rimSamples.Any())
            {
                if (isRimType)
                    Samples.Add(CreateHitSampleInfo(HitSampleInfo.HIT_CLAP));
                else
                {
                    foreach (var sample in rimSamples)
                        Samples.Remove(sample);
                }
            }
        }

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit(this)
        {
            StartTime = startTime,
            Samples = Samples
        };

        protected override void CopyFrom(HitObject other, IDictionary<object, object>? referenceLookup = null)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not Hit hit)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(Hit)}");

            Type = hit.Type;
            DisplayColour.Value = hit.DisplayColour.Value;
        }

        protected override HitObject CreateInstance() => new Hit();

        public class StrongNestedHit : StrongNestedHitObject
        {
            public StrongNestedHit(TaikoHitObject parent)
                : base(parent)
            {
            }

            protected override HitObject CreateInstance() => new StrongNestedHit(null!);
        }
    }
}
