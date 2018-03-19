using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace Symcol.Rulesets.Core.Wiki
{
    public class WikiSubSectionHeader : OsuSpriteText
    {
        public WikiSubSectionHeader(string text)
        {
            OsuColour osu = new OsuColour();
            Colour = osu.Pink;
            Text = text;
            TextSize = 24;
            Font = @"Exo2.0-BoldItalic";
            Margin = new MarginPadding
            {
                Vertical = 10
            };
        }
    }
}
