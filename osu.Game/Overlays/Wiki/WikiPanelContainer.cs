// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays.Wiki.Markdown;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Wiki
{
    public class WikiPanelContainer : Container
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        public string Text;

        [BackgroundDependencyLoader]
        private void load()
        {
            Padding = new MarginPadding(3);
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 4,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(25),
                        Offset = new Vector2(0, 1),
                        Radius = 3,
                    },
                    Child = new Box
                    {
                        Colour = colourProvider.Background4,
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                new WikiPanelMarkdownContainer
                {
                    Text = Text,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            };
        }

        private class WikiPanelMarkdownContainer : WikiMarkdownContainer
        {
            public WikiPanelMarkdownContainer()
            {
                LineSpacing = 0;
                DocumentPadding = new MarginPadding(30);
                DocumentMargin = new MarginPadding(0);
            }
        }
    }
}
