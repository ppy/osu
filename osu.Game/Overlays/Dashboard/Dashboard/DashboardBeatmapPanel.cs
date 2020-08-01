// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Dashboard.Dashboard
{
    public abstract class DashboardBeatmapPanel : OsuClickableContainer
    {
        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; }

        [Resolved(canBeNull: true)]
        private BeatmapSetOverlay beatmapOverlay { get; set; }

        protected readonly BeatmapSetInfo SetInfo;

        private Box background;
        private SpriteIcon chevron;

        protected DashboardBeatmapPanel(BeatmapSetInfo setInfo)
        {
            SetInfo = setInfo;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 60;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourProvider.Background3,
                    Alpha = 0
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 10 },
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
                                    Child = new UpdateableBeatmapSetCover
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        BeatmapSet = SetInfo
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
                                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                                Text = SetInfo.Metadata.Title
                                            },
                                            new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Truncate = true,
                                                Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                                Text = SetInfo.Metadata.Artist
                                            },
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(3, 0),
                                                Margin = new MarginPadding { Top = 2 },
                                                Children = new[]
                                                {
                                                    new LinkFlowContainer(f => f.Font = OsuFont.GetFont(size: 10, weight: FontWeight.SemiBold))
                                                    {
                                                        AutoSizeAxes = Axes.Both
                                                    }.With(c =>
                                                    {
                                                        c.AddText("by ");
                                                        c.AddUserLink(SetInfo.Metadata.Author);
                                                    }),
                                                    CreateInfo()
                                                }
                                            }
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
                if (SetInfo.OnlineBeatmapSetID.HasValue)
                    beatmapOverlay?.FetchAndShowBeatmapSet(SetInfo.OnlineBeatmapSetID.Value);
            };
        }

        protected abstract Drawable CreateInfo();

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            background.FadeIn(200, Easing.OutQuint);
            chevron.FadeColour(ColourProvider.Light1, 200, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            background.FadeOut(200, Easing.OutQuint);
            chevron.FadeColour(ColourProvider.Foreground1, 200, Easing.OutQuint);
        }
    }
}
