// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelGroupRankDisplay : Panel
    {
        public const float HEIGHT = PanelGroup.HEIGHT;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Drawable iconContainer = null!;
        private Box backgroundBorder = null!;
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
                Alpha = 0f,
                Child = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.ChevronDown,
                    Size = new Vector2(12),
                },
            };

            Background = backgroundBorder = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Highlight1,
            };

            AccentColour = colourProvider.Highlight1;
            Content.Children = new Drawable[]
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
                    Colour = ColourInfo.GradientHorizontal(colourProvider.Background6, colourProvider.Background5)
                },
                glow = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Colour = ColourInfo.GradientHorizontal(colourProvider.Highlight1, colourProvider.Highlight1.Opacity(0f)),
                },
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

        private Color4 rankColour;

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            var group = (RankDisplayGroupDefinition)Item.Model;
            ScoreRank rank = group.Rank;

            rankColour = OsuColour.ForRank(rank);

            AccentColour = rankColour;
            backgroundBorder.Colour = rankColour;
            contentBackground.Colour = rankColour.Darken(1f);
            glow.Colour = ColourInfo.GradientHorizontal(rankColour, rankColour.Opacity(0f));

            switch (rank)
            {
                case ScoreRank.SH:
                case ScoreRank.XH:
                    starRatingText.Colour = DrawableRank.GetRankLetterColour(rank);
                    iconContainer.Colour = colourProvider.Background5;
                    break;

                case ScoreRank.X:
                case ScoreRank.S:
                    starRatingText.Colour = DrawableRank.GetRankLetterColour(rank);
                    iconContainer.Colour = colourProvider.Background5;
                    break;

                case ScoreRank.F:
                    starRatingText.Colour = DrawableRank.GetRankLetterColour(rank);
                    iconContainer.Colour = colourProvider.Content1;
                    break;

                default:
                    starRatingText.Colour = Color4.White;
                    iconContainer.Colour = colourProvider.Background5;
                    break;
            }

            starRatingText.Text = group.Title;

            ColourInfo colour = ColourInfo.GradientHorizontal(rankColour.Darken(0.6f), rankColour.Darken(0.8f));

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

        public override MenuItem[] ContextMenuItems
        {
            get
            {
                if (Item == null)
                    return Array.Empty<MenuItem>();

                return new MenuItem[]
                {
                    new OsuMenuItem(Expanded.Value ? WebCommonStrings.ButtonsCollapse.ToSentence() : WebCommonStrings.ButtonsExpand.ToSentence(), MenuItemType.Highlighted, () => TriggerClick())
                };
            }
        }
    }
}
