using osu.Core.Containers.Text;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Core.Wiki.Sections.Subsection
{
    public class WikiSubSectionLinkHeader : LinkOsuSpriteText
    {
        public WikiSubSectionLinkHeader(string text, string url, string tooltip = "")
        {
            Tooltip = tooltip;
            Url = url;
            OsuColour osu = new OsuColour();
            IdleColour = osu.Pink;
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
