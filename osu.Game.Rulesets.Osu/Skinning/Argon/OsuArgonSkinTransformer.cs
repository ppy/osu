// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public class OsuArgonSkinTransformer : ISkin
    {
        public OsuArgonSkinTransformer(ISkin skin)
        {
        }

        public Drawable? GetDrawableComponent(ISkinComponent component)
        {
            if (component is OsuSkinComponent osuComponent)
            {
                switch (osuComponent.Component)
                {
                    case OsuSkinComponents.HitCircle:
                    case OsuSkinComponents.SliderHeadHitCircle:
                        return new ArgonMainCirclePiece();
                }
            }

            return null;
        }

        public Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            return null;
        }

        public ISample? GetSample(ISampleInfo sampleInfo)
        {
            return null;
        }

        public IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup) where TLookup : notnull where TValue : notnull
        {
            return null;
        }
    }
}
