// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultSkin : Skin
    {
        public DefaultSkin()
            : base(SkinInfo.Default)
        {
            Configuration = new SkinConfiguration
            {
                ComboColours =
                {
                    new Color4(17, 136, 170, 255),
                    new Color4(102, 136, 0, 255),
                    new Color4(204, 102, 0, 255),
                    new Color4(121, 9, 13, 255)
                }
            };
        }

        public override Drawable GetDrawableComponent(string componentName) => null;

        public override Texture GetTexture(string componentName) => null;

        public override SampleChannel GetSample(string sampleName) => null;
    }
}
