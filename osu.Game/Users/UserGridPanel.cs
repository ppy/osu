// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile.Header.Components;
using osuTK;

namespace osu.Game.Users
{
    /// <summary>
    /// A user "card", commonly used in a grid layout or in popovers.
    /// Comes with a preset height, but width must be specified.
    /// </summary>
    public partial class UserGridPanel : ExtendedUserPanel
    {
        private const int margin = 10;

        public UserGridPanel(APIUser user)
            : base(user)
        {
            Height = 120;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.FadeTo(0.3f);
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
                        new Dimension(GridSizeMode.Absolute, margin),
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
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Left = margin },
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
                            }
                        },
                        new[]
                        {
                            Empty(),
                            Empty()
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
