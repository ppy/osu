// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Wiki
{
    public class WikiSidebar : OverlaySidebar
    {
        private FillFlowContainer tableOfContents;

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            Direction = FillDirection.Vertical,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "CONTENTS",
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                },
                tableOfContents = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            },
        };

        public void AddToc(string title, MarkdownHeading heading, int level)
        {
            switch (level)
            {
                case 2:
                    tableOfContents.Add(new OsuSpriteText
                    {
                        Text = title,
                        Font = OsuFont.GetFont(size: 15),
                    });
                    break;
            }
        }
    }
}
