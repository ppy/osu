using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultSkin : Skin
    {
        public DefaultSkin()
            : base("Default")
        {
        }

        public override Drawable GetComponent(string componentName) => null;
    }
}
