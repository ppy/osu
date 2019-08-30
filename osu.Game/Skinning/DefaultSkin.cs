// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    public class DefaultSkin : Skin
    {
        public DefaultSkin()
            : base(SkinInfo.Default)
        {
            Configuration = new SkinConfiguration();
        }

        public override Drawable GetDrawableComponent(ISkinComponent component) => null;

        public override Texture GetTexture(string componentName) => null;

        public override SampleChannel GetSample(ISampleInfo sampleInfo) => null;
    }
}
