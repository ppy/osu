// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using Markdig.Syntax;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Overlays.Wiki.Markdown;

namespace osu.Game.Overlays.Wiki
{
    public partial class WikiArticlePage : CompositeDrawable
    {
        public Container SidebarContainer { get; }

        public WikiArticlePage(string currentPath, string markdown)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            WikiSidebar sidebar;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        SidebarContainer = new Container
                        {
                            AutoSizeAxes = Axes.X,
                            Child = sidebar = new WikiSidebar(),
                        },
                        new ArticleMarkdownContainer
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
                                Right = WaveOverlayContainer.HORIZONTAL_PADDING,
                            },
                            OnAddHeading = sidebar.AddEntry,
                        }
                    },
                },
            };
        }

        private partial class ArticleMarkdownContainer : WikiMarkdownContainer
        {
            public Action<HeadingBlock, MarkdownHeading> OnAddHeading;

            protected override MarkdownHeading CreateHeading(HeadingBlock headingBlock)
            {
                var heading = base.CreateHeading(headingBlock);

                OnAddHeading(headingBlock, heading);

                return heading;
            }
        }
    }
}
