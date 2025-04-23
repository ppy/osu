// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelGroup : Panel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT * 1.2f;

        private Drawable iconContainer = null!;
        private OsuSpriteText titleText = null!;
        private TrianglesV2 triangles = null!;
        private OsuSpriteText countText = null!;
        private Box glow = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = HEIGHT;

            Icon = iconContainer = new Container
            {
                AlwaysPresent = true,
                RelativeSizeAxes = Axes.Y,
                Child = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.ChevronDown,
                    Size = new Vector2(12),
                    Colour = colourProvider.Background3,
                },
            };
            Background = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5,
                    },
                    triangles = new TrianglesV2
                    {
                        RelativeSizeAxes = Axes.Both,
                        Thickness = 0.02f,
                        SpawnRatio = 0.6f,
                        Colour = ColourInfo.GradientHorizontal(colourProvider.Background6, colourProvider.Background5)
                    },
                    glow = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.5f,
                        Colour = ColourInfo.GradientHorizontal(colourProvider.Highlight1, colourProvider.Highlight1.Opacity(0f)),
                    },
                },
            };
            AccentColour = colourProvider.Highlight1;
            Content.Children = new Drawable[]
            {
                titleText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = OsuFont.Style.Heading2,
                    UseFullGlyphHeight = false,
                    X = 10f,
                },
                new CircularContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Size = new Vector2(50f, 14f),
                    Margin = new MarginPadding { Right = 20f },
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.7f),
                        },
                        countText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold),
                            UseFullGlyphHeight = false,
                        }
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => onExpanded(), true);
        }

        private void onExpanded()
        {
            const float duration = 500;

            iconContainer.ResizeWidthTo(Expanded.Value ? 20f : 5f, duration, Easing.OutQuint);
            iconContainer.FadeTo(Expanded.Value ? 1f : 0f, duration, Easing.OutQuint);

            ColourInfo colour = Expanded.Value
                ? ColourInfo.GradientHorizontal(colourProvider.Highlight1.Opacity(0.25f), colourProvider.Highlight1.Opacity(0f))
                : ColourInfo.GradientHorizontal(colourProvider.Background6, colourProvider.Background5);

            triangles.FadeColour(colour, duration, Easing.OutQuint);
            glow.FadeTo(Expanded.Value ? 0.4f : 0, duration, Easing.OutQuint);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            GroupDefinition group = (GroupDefinition)Item.Model;

            titleText.Text = group.Title;
            countText.Text = Item.NestedItemCount.ToString("N0");
        }
    }
}
