using osu.Framework.Graphics;
using osu.Game.Graphics;
using Symcol.Rulesets.Core.Containers;

namespace Symcol.Rulesets.Core.Wiki
{
    public class WikiSubSectionLinkHeader : LinkText
    {
        public override string Tooltip => tooltip;

        private string tooltip = "";

        public WikiSubSectionLinkHeader(string text, string url, string tooltip = "")
        {
            this.tooltip = tooltip;
            Url = url;
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
