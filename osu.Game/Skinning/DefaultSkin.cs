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

        public override Drawable GetDrawableComponent(string componentName) => null;

        public override SampleChannel GetSample(string sampleName) => null;
    }
}
