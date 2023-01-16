// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderBoardScoreV2 : OsuClickableContainer
    {
        private readonly ScoreInfo score;

        private const int HEIGHT = 60;
        private const int corner_radius = 10;
        private const int transition_duration = 200;

        private readonly int? rank;

        private readonly bool isPersonalBest;

        private Colour4 foregroundColour;
        private Colour4 backgroundColour;

        private static readonly Vector2 shear = new Vector2(0.15f, 0);

        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private Container content = null!;
        private Box background = null!;
        private Box foreground = null!;

        public LeaderBoardScoreV2(ScoreInfo score, int? rank, bool isPersonalBest = false)
        {
            this.score = score;
            this.rank = rank;
            this.isPersonalBest = isPersonalBest;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foregroundColour = isPersonalBest ? colourProvider.Background1 : colourProvider.Background5;
            backgroundColour = isPersonalBest ? colourProvider.Background2 : colourProvider.Background4;

            Shear = shear;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
            Child = content = new Container
            {
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 65),
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 176)
                        },
                        Content = new[]
                        {
                            new[]
                            {
                                new RankLabel(rank)
                                {
                                    Shear = -shear,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 35
                                },
                                createCentreContent(),
                                Empty(),
                            }
                        }
                    }
                }
            };
        }

        private Container createCentreContent() =>
            new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    foreground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = foregroundColour
                    }
                }
            };

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            foreground.FadeColour(IsHovered ? foregroundColour.Lighten(0.2f) : foregroundColour, transition_duration, Easing.OutQuint);
            background.FadeColour(IsHovered ? backgroundColour.Lighten(0.2f) : backgroundColour, transition_duration, Easing.OutQuint);
        }

        private partial class RankLabel : Container, IHasTooltip
        {
            public RankLabel(int? rank)
            {
                if (rank >= 1000)
                    TooltipText = $"#{rank:N0}";

                Child = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold, italics: true),
                    Text = rank == null ? "-" : rank.Value.FormatRank().Insert(0, "#")
                };
            }

            public LocalisableString TooltipText { get; }
        }
    }
}
