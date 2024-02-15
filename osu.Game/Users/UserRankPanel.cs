// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Users
{
    /// <summary>
    /// User card that shows user's global and country ranks in the bottom.
    /// Meant to be used in the toolbar login overlay.
    /// </summary>
    public partial class UserRankPanel : UserPanel
    {
        private const int padding = 10;
        private const int main_content_height = 80;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private ProfileValueDisplay globalRankDisplay = null!;
        private ProfileValueDisplay countryRankDisplay = null!;
        private ProfileValueDisplay ppDisplay = null!;

        private readonly IBindable<UserStatistics?> statistics = new Bindable<UserStatistics?>();

        public UserRankPanel(APIUser user)
            : base(user)
        {
            AutoSizeAxes = Axes.Y;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BorderColour = ColourProvider?.Light1 ?? Colours.GreyVioletLighter;

            statistics.BindTo(api.Statistics);
            statistics.BindValueChanged(stats =>
            {
                globalRankDisplay.Content = stats.NewValue?.GlobalRank?.ToLocalisableString("\\##,##0") ?? "-";
                countryRankDisplay.Content = stats.NewValue?.CountryRank?.ToLocalisableString("\\##,##0") ?? "-";
                ppDisplay.Content = stats.NewValue?.PP?.ToLocalisableString("#,##0") ?? "0";
            }, true);
        }

        protected override Drawable CreateLayout()
        {
            FillFlowContainer details;

            var layout = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Name = "Main content",
                        RelativeSizeAxes = Axes.X,
                        Height = main_content_height,
                        CornerRadius = 10,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new UserCoverBackground
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                User = User,
                                Alpha = 0.3f
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(padding),
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(),
                                },
                                RowDimensions = new[]
                                {
                                    new Dimension()
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        CreateAvatar().With(avatar =>
                                        {
                                            avatar.Size = new Vector2(60);
                                            avatar.Masking = true;
                                            avatar.CornerRadius = 6;
                                        }),
                                        new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Left = padding },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension()
                                            },
                                            RowDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.AutoSize),
                                                new Dimension()
                                            },
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    details = new FillFlowContainer
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Direction = FillDirection.Horizontal,
                                                        Spacing = new Vector2(6),
                                                        Children = new Drawable[]
                                                        {
                                                            CreateFlag(),
                                                            // supporter icon is being added later
                                                        }
                                                    }
                                                },
                                                new Drawable[]
                                                {
                                                    CreateUsername().With(username =>
                                                    {
                                                        username.Anchor = Anchor.CentreLeft;
                                                        username.Origin = Anchor.CentreLeft;
                                                    })
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new GridContainer
                    {
                        Name = "Bottom content",
                        Margin = new MarginPadding { Top = main_content_height },
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Left = 80, Right = padding, Vertical = padding },
                        ColumnDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension()
                        },
                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize), new Dimension(GridSizeMode.AutoSize) },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                globalRankDisplay = new ProfileValueDisplay(true)
                                {
                                    Title = UsersStrings.ShowRankGlobalSimple,
                                    Margin = new MarginPadding { Bottom = padding }
                                },
                                countryRankDisplay = new ProfileValueDisplay(true)
                                {
                                    Title = UsersStrings.ShowRankCountrySimple
                                }
                            },
                            new Drawable[]
                            {
                                ppDisplay = new ProfileValueDisplay
                                {
                                    Title = "pp"
                                },
                                new TotalPlayTime
                                {
                                    UserStatistics = { BindTarget = statistics }
                                }
                            }
                        }
                    }
                }
            };

            if (User.IsSupporter)
            {
                details.Add(new SupporterIcon
                {
                    Height = 26,
                    SupportLevel = User.SupportLevel
                });
            }

            return layout;
        }

        protected override bool OnHover(HoverEvent e)
        {
            BorderThickness = 2;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            BorderThickness = 0;
            base.OnHoverLost(e);
        }

        protected override Drawable? CreateBackground() => null;
    }
}
