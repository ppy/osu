// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    public abstract class Skin
    {
        public readonly SkinInfo SkinInfo;

        public abstract Drawable GetDrawableComponent(string componentName);

        public abstract SampleChannel GetSample(string sampleName);

        protected Skin(SkinInfo skin)
        {
            SkinInfo = skin;
        }
    }
}
