// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Screens.Select;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardCommonDisplay : Container
    {
        public readonly APIUser User;
        public readonly DateTimeOffset? Date;
        public readonly int? Rank;

        private const int corner_radius = BeatmapLeaderboardScore.CORNER_RADIUS;

        private FillFlowContainer fillFlowContainer = null!;
        private ClickableAvatar innerAvatar = null!;
        private Container rankOverlay = null!;

        private readonly bool sheared;

        public LeaderboardCommonDisplay(APIUser user, DateTimeOffset? date, int? rank = null, bool sheared = false)
        {
            User = user;
            Date = date;
            Rank = rank;

            this.sheared = sheared;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.Both;

            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            CornerRadius = corner_radius,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new DelayedLoadWrapper(innerAvatar = new ClickableAvatar(User)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Scale = new Vector2(1.1f),
                                    Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                    RelativeSizeAxes = Axes.Both,
                                })
                                {
                                    RelativeSizeAxes = Axes.None,
                                    Size = new Vector2(BeatmapLeaderboardScore.HEIGHT)
                                },
                                rankOverlay = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Colour4.Black.Opacity(0.5f),
                                        },
                                        new LeaderboardRankLabel(Rank, sheared, false)
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                        },
                                    }
                                }
                            },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding { Horizontal = corner_radius },
                            Children = new Drawable[]
                            {
                                fillFlowContainer = new FillFlowContainer
                                {
                                    Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5),
                                    AutoSizeAxes = Axes.Both,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new UpdateableFlag(User.CountryCode)
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(20, 14),
                                        },
                                        new UpdateableTeamFlag(User.Team)
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(30, 15),
                                        },
                                    },
                                },
                                new TruncatingSpriteText
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                                    Text = User.Username,
                                    Font = OsuFont.Style.Heading2,
                                }
                            }
                        },
                    },
                }
            };

            innerAvatar.OnLoadComplete += d => d.FadeInFromZero(200);

            if (Date != null)
            {
                fillFlowContainer.Add(new DateLabel(Date.Value)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Colour = colourProvider.Content2,
                    UseFullGlyphHeight = false,
                });
            }
        }

        public void UpdateRankOverlayState(bool shouldBeVisible, double duration)
        {
            if (shouldBeVisible)
                rankOverlay.FadeIn(duration, Easing.OutQuint);
            else
                rankOverlay.FadeOut(duration, Easing.OutQuint);
        }

        private partial class DateLabel : DrawableDate
        {
            public DateLabel(DateTimeOffset date)
                : base(date)
            {
                Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold);
            }

            protected override LocalisableString Format() => Date.ToShortRelativeTime(TimeSpan.FromSeconds(30));
        }
    }
}
