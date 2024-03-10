// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Markdig.Syntax;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Online.API;
using osu.Game.Overlays.Wiki.Markdown;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Wiki
{
    public partial class WikiPanelContainer : CompositeDrawable
    {
        private const float padding = 3;

        private readonly string text;
        private readonly bool isFullWidth;

        public WikiPanelContainer(string text, bool isFullWidth = false)
        {
            this.text = text;
            this.isFullWidth = isFullWidth;
        }

        private PanelBackground background;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                background = new PanelBackground
                {
                    BypassAutoSizeAxes = Axes.Both
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(padding),
                    Child = new WikiPanelMarkdownContainer(isFullWidth)
                    {
                        CurrentPath = $@"{api.WebsiteRootUrl}/wiki/",
                        Text = text,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    }
                }
            };
        }

        protected override void Update()
        {
            base.Update();
            background.Size = Parent!.DrawSize * new Vector2(Size.X, 1);
        }

        private partial class PanelBackground : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Padding = new MarginPadding(padding);
                InternalChild = new Container
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
                    }
                };
            }
        }

        private partial class WikiPanelMarkdownContainer : WikiMarkdownContainer
        {
            private readonly bool isFullWidth;

            public WikiPanelMarkdownContainer(bool isFullWidth)
            {
                this.isFullWidth = isFullWidth;

                LineSpacing = 0;
                DocumentPadding = new MarginPadding(30);
                DocumentMargin = new MarginPadding(0);
            }

            public override SpriteText CreateSpriteText() => base.CreateSpriteText().With(t => t.Font = t.Font.With(Typeface.Torus, weight: FontWeight.Bold));

            public override OsuMarkdownTextFlowContainer CreateTextFlow() => base.CreateTextFlow().With(f => f.TextAnchor = Anchor.TopCentre);

            protected override MarkdownParagraph CreateParagraph(ParagraphBlock paragraphBlock, int level)
                => base.CreateParagraph(paragraphBlock, level).With(p => p.Margin = new MarginPadding { Bottom = 10 });

            protected override MarkdownHeading CreateHeading(HeadingBlock headingBlock) => new WikiPanelHeading(headingBlock)
            {
                IsFullWidth = isFullWidth,
            };
        }

        private partial class WikiPanelHeading : OsuMarkdownHeading
        {
            public bool IsFullWidth;

            public WikiPanelHeading(HeadingBlock headingBlock)
                : base(headingBlock)
            {
                Margin = new MarginPadding { Bottom = 40 };
            }

            public override MarkdownTextFlowContainer CreateTextFlow() => base.CreateTextFlow().With(f =>
            {
                f.Anchor = Anchor.TopCentre;
                f.Origin = Anchor.TopCentre;
                f.TextAnchor = Anchor.TopCentre;
            });

            protected override FontWeight GetFontWeightByLevel(int level) => FontWeight.Light;

            protected override float GetFontSizeByLevel(int level) => base.GetFontSizeByLevel(IsFullWidth ? level : 3);
        }
    }
}
