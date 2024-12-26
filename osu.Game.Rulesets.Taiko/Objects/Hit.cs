// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Hit : TaikoStrongableHitObject, IHasDisplayColour
    {
        /// <summary>
        /// The <see cref="HitType"/> that actuates this <see cref="Hit"/>.
        /// </summary>
        public HitType Type { get; protected set; }

        public Bindable<Color4> DisplayColour { get; } = new Bindable<Color4>(COLOUR_CENTRE);

        public static readonly Color4 COLOUR_CENTRE = Color4Extensions.FromHex(@"bb1177");
        public static readonly Color4 COLOUR_RIM = Color4Extensions.FromHex(@"2299bb");

        public Hit(HitType type, IList<HitSampleInfo>? samples = null)
        {
            changeType(type);
            if (samples is not null) Samples = samples;
            updateSamplesFromType();
            UpdateStrongSamplesFromType();
            SamplesBindable.BindCollectionChanged((_, e) =>
            {
                updateSamplesFromType();
                updateTypeFromSamples();
            });
        }

        public static Hit? InvertType(TaikoHitObject obj)
        {
            switch (obj)
            {
                case HitRim rim:
                    return new HitCentre(rim);
                case HitCentre centre:
                    return new HitRim(centre);
                case Hit hit:
                    hit.changeType(hit.Type == HitType.Centre ? HitType.Rim : HitType.Centre);
                    break;
            }
            return null;
        }

        private void changeType(HitType type)
        {
            Type = type;
            DisplayColour.Value = Type == HitType.Centre ? COLOUR_CENTRE : COLOUR_RIM;
        }

        private void updateTypeFromSamples()
        {
            HitType newType = getRimSamples().Any() ? HitType.Rim : HitType.Centre;
            if ((newType != Type) && (this is HitCentre || this is HitRim))
                throw new ArgumentException("HitCentre/HitRim was dynamically changed between each other!");
            Type = newType;
        }

        public static HitType SampleType(IList<HitSampleInfo> samples)
            => samples.Where(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE).Any() ? HitType.Rim : HitType.Centre;

        public static Hit CreateConcreteBySample(IList<HitSampleInfo> samples)
        {
            switch (SampleType(samples))
            {
                case HitType.Centre: return new HitCentre(samples);
                case HitType.Rim: return new HitRim(samples);
                default: throw new NotImplementedException("Unimplemented hit type!");
            }
        }

        /// <summary>
        /// Returns an array of any samples which would cause this object to be a "rim" type hit.
        /// </summary>
        private HitSampleInfo[] getRimSamples() => Samples.Where(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE).ToArray();

        private void updateSamplesFromType()
        {
            var rimSamples = getRimSamples();

            bool isRimType = Type == HitType.Rim;
            int volume = Samples.Any() ? Samples.First().Volume : 100;

            if (isRimType != rimSamples.Any())
            {
                if (isRimType)
                    Samples.Add(CreateHitSampleInfo(HitSampleInfo.HIT_CLAP).With(newVolume: volume));
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

        public class StrongNestedHit : StrongNestedHitObject
        {
            public StrongNestedHit(TaikoHitObject parent)
                : base(parent)
            {
            }
        }
    }

    public class HitRim : Hit
    {
        public HitRim(IList<HitSampleInfo>? samples = null) : base(HitType.Rim, samples) { }
        public HitRim(HitCentre opposite) : base(HitType.Rim, opposite.Samples)
        {
            // TODO: is there a better way?
            StartTime = opposite.StartTime;
            IsStrong = opposite.IsStrong;
            HitWindows = opposite.HitWindows;
        }
    }

    public class HitCentre : Hit
    {
        public HitCentre(IList<HitSampleInfo>? samples = null) : base(HitType.Centre, samples) { }
        public HitCentre(HitRim opposite) : base(HitType.Centre, opposite.Samples)
        {
            // TODO: is there a better way?
            StartTime = opposite.StartTime;
            IsStrong = opposite.IsStrong;
            HitWindows = opposite.HitWindows;
        }
    }
}
