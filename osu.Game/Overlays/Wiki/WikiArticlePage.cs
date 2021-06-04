// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Wiki.Markdown;

namespace osu.Game.Overlays.Wiki
{
    public class WikiArticlePage : GridContainer
    {
        public Container SidebarContainer { get; }

        public WikiArticlePage(string currentPath, string markdown)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize),
            };
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(),
            };
            Content = new[]
            {
                new Drawable[]
                {
                    SidebarContainer = new Container
                    {
                        AutoSizeAxes = Axes.X,
                        Child = new WikiSidebar(),
                    },
                    new WikiMarkdownContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        CurrentPath = currentPath,
                        Text = markdown,
                        DocumentMargin = new MarginPadding(0),
                        DocumentPadding = new MarginPadding
                        {
                            Vertical = 20,
                            Left = 30,
                            Right = 50,
                        },
                    }
                },
            };
        }
    }
}
