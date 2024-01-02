// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
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
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, padding),
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, padding),
                                },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, padding),
                                    new Dimension(GridSizeMode.AutoSize),
                                },
                                Content = new[]
                                {
                                    new[]
                                    {
                                        Empty(),
                                        Empty(),
                                        Empty(),
                                        Empty()
                                    },
                                    new[]
                                    {
                                        Empty(),
                                        CreateAvatar().With(avatar =>
                                        {
                                            avatar.Size = new Vector2(60);
                                            avatar.Masking = true;
                                            avatar.CornerRadius = 6;
                                        }),
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Left = padding },
                                            Child = new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
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
                                        },
                                        Empty()
                                    }
                                }
                            }
                        }
                    },
                    new Container
                    {
                        Name = "Bottom content",
                        Margin = new MarginPadding { Top = main_content_height },
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Left = 80, Vertical = padding },
                        Child = new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension()
                            },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new ProfileValueDisplay(true)
                                    {
                                        Title = UsersStrings.ShowRankGlobalSimple,
                                        Content = User.Statistics?.GlobalRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-"
                                    },
                                    new ProfileValueDisplay(true)
                                    {
                                        Title = UsersStrings.ShowRankCountrySimple,
                                        Content = User.Statistics?.CountryRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-"
                                    }
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
