// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultSkin : Skin
    {
        public DefaultSkin()
            : base(SkinInfo.Default)
        {
            Configuration = new DefaultSkinConfiguration();
        }

        public override Drawable GetDrawableComponent(ISkinComponent component) => null;

        public override Texture GetTexture(string componentName) => null;

        public override SampleChannel GetSample(ISampleInfo sampleInfo) => null;

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                // todo: this code is pulled from LegacySkin and should not exist.
                // will likely change based on how databased storage of skin configuration goes.
                case GlobalSkinColours global:
                    switch (global)
                    {
                        case GlobalSkinColours.ComboColours:
                            return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>>(Configuration.ComboColours));
                    }

                    break;
            }

            return null;
        }
    }
}
