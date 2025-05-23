// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelGroupStarDifficulty : Panel
    {
        public const float HEIGHT = PanelGroup.HEIGHT;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Drawable iconContainer = null!;
        private Box contentBackground = null!;
        private OsuSpriteText starRatingText = null!;
        private CircularContainer countPill = null!;
        private OsuSpriteText countText = null!;
        private TrianglesV2 triangles = null!;
        private Box glow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = PanelGroup.HEIGHT;

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
                },
            };
            Background = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    contentBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    triangles = new TrianglesV2
                    {
                        RelativeSizeAxes = Axes.Both,
                        Thickness = 0.02f,
                        SpawnRatio = 0.6f,
                    },
                    glow = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.5f,
                    },
                },
            };
            AccentColour = colourProvider.Highlight1;
            Content.Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(10f, 0f),
                    Margin = new MarginPadding { Left = 10f },
                    Children = new Drawable[]
                    {
                        starRatingText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            UseFullGlyphHeight = false,
                            Font = OsuFont.Style.Heading2,
                        }
                    }
                },
                countPill = new CircularContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Size = new Vector2(50f, 14f),
                    Margin = new MarginPadding { Right = 30f },
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
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => onExpanded(), true);
        }

        private Color4 ratingColour;

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            var group = (StarDifficultyGroupDefinition)Item.Model;
            int starNumber = (int)group.Difficulty.Stars;

            ratingColour = starNumber >= 9 ? OsuColour.Gray(0.2f) : colours.ForStarDifficulty(starNumber);

            AccentColour = ratingColour;
            contentBackground.Colour = ratingColour.Darken(1f);
            glow.Colour = ColourInfo.GradientHorizontal(ratingColour, ratingColour.Opacity(0f));

            switch (starNumber)
            {
                case 0:
                    starRatingText.Text = @"Below 1 Star";
                    break;

                case 1:
                    starRatingText.Text = @"1 Star";
                    break;

                default:
                    starRatingText.Text = $"{starNumber} Stars";
                    break;
            }

            iconContainer.Colour = starNumber >= 7 ? colourProvider.Content1 : colourProvider.Background5;
            starRatingText.Colour = colourProvider.Content1;
            starRatingText.Text = group.Title;

            ColourInfo colour;

            if (starNumber >= 8)
                colour = ColourInfo.GradientHorizontal(ratingColour, ratingColour.Darken(0.2f));
            else
                colour = ColourInfo.GradientHorizontal(ratingColour.Darken(0.6f), ratingColour.Darken(0.8f));

            triangles.Colour = colour;

            countText.Text = Item.NestedItemCount.ToLocalisableString(@"N0");

            onExpanded();
        }

        private void onExpanded()
        {
            const float duration = 500;

            iconContainer.ResizeWidthTo(Expanded.Value ? 20f : 5f, duration, Easing.OutQuint);
            iconContainer.FadeTo(Expanded.Value ? 1f : 0f, duration, Easing.OutQuint);

            glow.FadeTo(Expanded.Value ? 0.4f : 0, duration, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            // Move the count pill in the opposite direction to keep it pinned to the screen regardless of the X position of TopLevelContent.
            countPill.X = -TopLevelContent.X;
        }
    }
}
