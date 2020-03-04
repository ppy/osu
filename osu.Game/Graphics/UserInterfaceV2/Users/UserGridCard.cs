// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2.Users
{
    public class UserGridCard : UserCard
    {
        private const int margin = 10;

        public UserGridCard(User user)
            : base(user)
        {
            Size = new Vector2(290, 120);
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.FadeTo(User.IsOnline ? 0.6f : 0.7f);
        }

        protected override Drawable CreateLayout()
        {
            FillFlowContainer details;

            var layout = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(margin),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
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
                            CreateAvatar().With(avatar =>
                            {
                                avatar.Size = new Vector2(60);
                                avatar.Margin = new MarginPadding { Bottom = margin };
                                avatar.Masking = true;
                                avatar.CornerRadius = 6;
                            }),
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 7),
                                Margin = new MarginPadding { Left = margin },
                                Children = new Drawable[]
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
                                    },
                                    CreateUsername(),
                                }
                            }
                        },
                        new Drawable[]
                        {
                            CreateStatusIcon().With(icon =>
                            {
                                icon.Anchor = Anchor.Centre;
                                icon.Origin = Anchor.Centre;
                            }),
                            CreateStatusMessage(false).With(message =>
                            {
                                message.Anchor = Anchor.CentreLeft;
                                message.Origin = Anchor.CentreLeft;
                                message.Margin = new MarginPadding { Left = margin };
                            })
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
    }
}
