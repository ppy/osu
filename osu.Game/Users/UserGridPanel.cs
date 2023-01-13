// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Users
{
    public partial class UserGridPanel : ExtendedUserPanel
    {
        private FillFlowContainer details;
        private LevelBadge level;
        private Box hoverDim;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        protected override bool ShouldCreateUserCoverBackground { get; set; } = false;

        private const int main_panel_heigth = 60;

        public UserGridPanel(APIUser user)
            : base(user)
        {
            AutoSizeAxes = Axes.Y;
            CornerRadius = 10;
        }

        protected override Drawable CreateLayout()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Name = "Main content",
                        RelativeSizeAxes = Axes.X,
                        Height = main_panel_heigth,
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
                                Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0f), Color4.White.Opacity(0.4f))
                            },
                            hoverDim = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.Black,
                                Alpha = 0
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, 60),
                                    new Dimension(GridSizeMode.Absolute, 14),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 22),
                                    new Dimension(GridSizeMode.Absolute, 60)
                                },
                                RowDimensions = new[] { new Dimension() },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        CreateAvatar().With(avatar =>
                                        {
                                            avatar.Anchor = Anchor.CentreLeft;
                                            avatar.Origin = Anchor.CentreLeft;
                                            avatar.Size = new Vector2(main_panel_heigth);
                                            avatar.Masking = true;
                                            avatar.CornerRadius = 10;
                                        }),
                                        new CircularContainer
                                        {
                                            Name = "User colour pillar",
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Masking = true,
                                            Width = 4,
                                            Height = 40,
                                            Child = new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = string.IsNullOrEmpty(User.Colour) ? Color4Extensions.FromHex("0087ca") : Color4Extensions.FromHex(User.Colour)
                                            }
                                        },
                                        new FillFlowContainer
                                        {
                                            Name = "User details",
                                            RelativeSizeAxes = Axes.Y,
                                            Margin = new MarginPadding { Left = 5 },
                                            Padding = new MarginPadding { Vertical = 8 },
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(10),
                                            Children = new Drawable[]
                                            {
                                                CreateUsername().With(username =>
                                                {
                                                    username.Anchor = Anchor.TopLeft;
                                                    username.Origin = Anchor.TopLeft;
                                                }),
                                                details = new FillFlowContainer
                                                {
                                                    Anchor = Anchor.TopLeft,
                                                    Origin = Anchor.TopLeft,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(2)
                                                }
                                            }
                                        },
                                        CreateFlag().With(flag =>
                                        {
                                            flag.Anchor = Anchor.Centre;
                                            flag.Origin = Anchor.Centre;
                                            flag.Size = new Vector2(22, 15);
                                        }),
                                        level = new LevelBadge
                                        {
                                            Size = new Vector2(50),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new GridContainer
                    {
                        Name = "Bottom content",
                        Margin = new MarginPadding { Top = main_panel_heigth },
                        Height = 30,
                        RelativeSizeAxes = Axes.X,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 60),
                            new Dimension()
                        },
                        RowDimensions = new[] { new Dimension() },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                CreateStatusIcon().With(icon =>
                                {
                                    icon.Anchor = Anchor.Centre;
                                    icon.Origin = Anchor.Centre;
                                    icon.Size = new Vector2(14);
                                }),
                                CreateStatusMessage(false).With(message =>
                                {
                                    message.Anchor = Anchor.CentreLeft;
                                    message.Origin = Anchor.CentreLeft;
                                    message.Margin = new MarginPadding { Left = 18 };
                                })
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var ruleset = rulesets?.GetRuleset(User.PlayMode);

            level.User.Value = new UserProfileData(User, ruleset.AsNonNull());

            // some API calls don't return PlayMode - we don't want to draw a ruleset icon in that case
            if (ruleset != null)
            {
                details.Add(ruleset.CreateInstance().CreateIcon().With(
                    icon =>
                    {
                        icon.Size = new Vector2(16);
                        icon.Margin = new MarginPadding { Right = 8 };
                    }));
            }

            details.Add(new GroupBadgeFlow
            {
                User = { Value = User }
            });

            if (User.IsSupporter)
            {
                details.Add(new SupporterIcon
                {
                    Height = 16,
                    SupportLevel = User.SupportLevel
                });
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverDim?.FadeTo(0.2f, 200);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverDim?.FadeTo(0f, 200);
        }
    }
}
