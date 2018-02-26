// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultSkin : Skin
    {
        public DefaultSkin()
            : base(SkinInfo.Default)
        {
        }

        public override Drawable GetDrawableComponent(string componentName)
        {
            return null;
        }

        public override SampleChannel GetSample(string sampleName)
        {
            return null;
        }
    }
}
