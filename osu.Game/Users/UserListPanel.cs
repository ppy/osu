// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Colour;
using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osu.Game.Overlays.Profile.Header.Components;

namespace osu.Game.Users
{
    public partial class UserListPanel : ExtendedUserPanel
    {
        public UserListPanel(APIUser user)
            : base(user)
        {
            RelativeSizeAxes = Axes.X;
            Height = 40;
            CornerRadius = 6;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Width = 0.5f;
            Background.Origin = Anchor.CentreRight;
            Background.Anchor = Anchor.CentreRight;
            Background.Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(1), Color4.White.Opacity(0.3f));
        }

        protected override Drawable CreateLayout()
        {
            FillFlowContainer details;

            var layout = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    details = new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            CreateAvatar().With(avatar =>
                            {
                                avatar.Anchor = Anchor.CentreLeft;
                                avatar.Origin = Anchor.CentreLeft;
                                avatar.Size = new Vector2(40);
                            }),
                            CreateFlag().With(flag =>
                            {
                                flag.Anchor = Anchor.CentreLeft;
                                flag.Origin = Anchor.CentreLeft;
                            }),
                            CreateUsername().With(username =>
                            {
                                username.Anchor = Anchor.CentreLeft;
                                username.Origin = Anchor.CentreLeft;
                                username.UseFullGlyphHeight = false;
                            })
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Margin = new MarginPadding { Right = 10 },
                        Children = new Drawable[]
                        {
                            CreateStatusIcon().With(icon =>
                            {
                                icon.Anchor = Anchor.CentreRight;
                                icon.Origin = Anchor.CentreRight;
                            }),
                            CreateStatusMessage(true).With(message =>
                            {
                                message.Anchor = Anchor.CentreRight;
                                message.Origin = Anchor.CentreRight;
                            })
                        }
                    }
                }
            };

            if (User.Groups != null)
            {
                details.Add(new GroupBadgeFlow
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    User = { Value = User }
                });
            }

            if (User.IsSupporter)
            {
                details.Add(new SupporterIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Height = 16,
                    SupportLevel = User.SupportLevel
                });
            }

            return layout;
        }
    }
}
