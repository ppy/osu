using M.Resources.Fonts;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Mf
{
    public partial class FontPreviewSpriteText : SpriteText
    {
        public FontPreviewSpriteText(Font font)
        {
            Font = new FontUsage($"{font.FamilyName}-Regular");
            Shadow = true;
        }
    }
}
