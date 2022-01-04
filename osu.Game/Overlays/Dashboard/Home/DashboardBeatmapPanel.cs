// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.Dashboard.Home
{
    public abstract class DashboardBeatmapPanel : OsuClickableContainer
    {
        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; }

        [Resolved(canBeNull: true)]
        private BeatmapSetOverlay beatmapOverlay { get; set; }

        protected readonly APIBeatmapSet BeatmapSet;

        private Box hoverBackground;
        private SpriteIcon chevron;

        protected DashboardBeatmapPanel(APIBeatmapSet beatmapSet)
        {
            BeatmapSet = beatmapSet;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 60;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = -10 },
                    Child = hoverBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourProvider.Background3,
                        Alpha = 0
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 70),
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize)
                        },
                        RowDimensions = new[]
                        {
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 6,
                                    Child = new UpdateableOnlineBeatmapSetCover
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        OnlineInfo = BeatmapSet
                                    }
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Horizontal = 10 },
                                    Child = new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Truncate = true,
                                                Font = OsuFont.GetFont(weight: FontWeight.Regular),
                                                Text = BeatmapSet.Title
                                            },
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Truncate = true,
                                                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular),
                                                Text = BeatmapSet.Artist
                                            },
                                            new LinkFlowContainer(f => f.Font = OsuFont.GetFont(size: 10, weight: FontWeight.Regular))
                                            {
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Spacing = new Vector2(3),
                                                Margin = new MarginPadding { Top = 2 }
                                            }.With(c =>
                                            {
                                                c.AddText("by");
                                                c.AddUserLink(BeatmapSet.Author);
                                                c.AddArbitraryDrawable(CreateInfo());
                                            })
                                        }
                                    }
                                },
                                chevron = new SpriteIcon
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Size = new Vector2(16),
                                    Icon = FontAwesome.Solid.ChevronRight,
                                    Colour = ColourProvider.Foreground1
                                }
                            }
                        }
                    }
                }
            };

            Action = () =>
            {
                if (BeatmapSet.OnlineID > 0)
                    beatmapOverlay?.FetchAndShowBeatmapSet(BeatmapSet.OnlineID);
            };
        }

        protected abstract Drawable CreateInfo();

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            hoverBackground.FadeIn(200, Easing.OutQuint);
            chevron.FadeColour(ColourProvider.Light1, 200, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            hoverBackground.FadeOut(200, Easing.OutQuint);
            chevron.FadeColour(ColourProvider.Foreground1, 200, Easing.OutQuint);
        }
    }
}
