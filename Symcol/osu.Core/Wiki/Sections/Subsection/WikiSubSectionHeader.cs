using osu.Core.Containers.Text;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Core.Wiki.Sections.Subsection
{
    public class WikiSubSectionHeader : ClickableOsuSpriteText
    {
        public WikiSubSectionHeader(string text = "")
        {
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
