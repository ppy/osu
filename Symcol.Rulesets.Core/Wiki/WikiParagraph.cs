using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;

namespace Symcol.Rulesets.Core.Wiki
{
    public class WikiParagraph : Container
    {
        public WikiParagraph(string text, float textsize = 20)
        {
            paragraphNoMarkdown(text, textsize);
        }

        public WikiParagraph(string text, bool markdown)
        {
            if (!markdown)
                paragraphNoMarkdown(text, 20);
            else
                paragraphMarkdown(text, 20);
        }
        public WikiParagraph(string text, float textsize, bool markdown)
        {
            if (!markdown)
                paragraphNoMarkdown(text, textsize);
            else
                paragraphMarkdown(text, textsize);
        }

        private void paragraphNoMarkdown(string text, float textsize)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Child = new OsuTextFlowContainer(t => { t.TextSize = textsize; })
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Text = text
            };
        }

        private void paragraphMarkdown(string text, float textsize)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Child = new OsuTextFlowContainer(t => { t.TextSize = textsize; })
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Text = text
            };
        }
    }
}
