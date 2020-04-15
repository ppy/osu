// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning
{
    public class TaikoLegacySkinTransformer : ISkin
    {
        private readonly ISkinSource source;

        public TaikoLegacySkinTransformer(ISkinSource source)
        {
            this.source = source;
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (!(component is TaikoSkinComponent taikoComponent))
                return null;

            switch (taikoComponent.Component)
            {
                case TaikoSkinComponents.DrumRollBody:
                    if (GetTexture("taiko-roll-middle") != null)
                        return new LegacyDrumRoll();

                    return null;

                case TaikoSkinComponents.InputDrum:
                    if (GetTexture("taiko-bar-left") != null)
                        return new LegacyInputDrum();

                    return null;

                case TaikoSkinComponents.CentreHit:
                case TaikoSkinComponents.RimHit:

                    if (GetTexture("taikohitcircle") != null)
                        return new LegacyHit(taikoComponent.Component);

                    return null;

                case TaikoSkinComponents.DrumRollTick:
                    return this.GetAnimation("sliderscorepoint", false, false);
            }

            return source.GetDrawableComponent(component);
        }

        public Texture GetTexture(string componentName) => source.GetTexture(componentName);

        public SampleChannel GetSample(ISampleInfo sampleInfo) => source.GetSample(new LegacyTaikoSampleInfo(sampleInfo));

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => source.GetConfig<TLookup, TValue>(lookup);

        private class LegacyTaikoSampleInfo : ISampleInfo
        {
            private readonly ISampleInfo source;

            public LegacyTaikoSampleInfo(ISampleInfo source)
            {
                this.source = source;
            }

            public IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (var name in source.LookupNames)
                        yield return $"taiko-{name}";

                    foreach (var name in source.LookupNames)
                        yield return name;
                }
            }

            public int Volume => source.Volume;
        }
    }
}
