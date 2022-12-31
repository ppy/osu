// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public partial class TopHeaderContainer : CompositeDrawable
    {
        private const float avatar_size = 110;

        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private UserCoverBackground cover = null!;
        private SupporterIcon supporterTag = null!;
        private UpdateableAvatar avatar = null!;
        private OsuSpriteText usernameText = null!;
        private ExternalLinkButton openUserExternally = null!;
        private OsuSpriteText titleText = null!;
        private UpdateableFlag userFlag = null!;
        private OsuSpriteText userCountryText = null!;
        private GroupBadgeFlow groupBadgeFlow = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        cover = new ProfileCoverBackground
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 250,
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Margin = new MarginPadding
                            {
                                Left = UserProfileOverlay.CONTENT_X_MARGIN,
                                Vertical = 10
                            },
                            Height = avatar_size,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                avatar = new UpdateableAvatar(isInteractive: false, showGuestOnNull: false)
                                {
                                    Size = new Vector2(avatar_size),
                                    Masking = true,
                                    CornerRadius = avatar_size * 0.25f,
                                },
                                new OsuContextMenuContainer
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Padding = new MarginPadding { Left = 10 },
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    new FillFlowContainer
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Direction = FillDirection.Horizontal,
                                                        Children = new Drawable[]
                                                        {
                                                            usernameText = new OsuSpriteText
                                                            {
                                                                Font = OsuFont.GetFont(size: 24, weight: FontWeight.Regular)
                                                            },
                                                            openUserExternally = new ExternalLinkButton
                                                            {
                                                                Margin = new MarginPadding { Left = 5 },
                                                                Anchor = Anchor.CentreLeft,
                                                                Origin = Anchor.CentreLeft,
                                                            },
                                                        }
                                                    },
                                                    titleText = new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 18, weight: FontWeight.Regular)
                                                    },
                                                    groupBadgeFlow = new GroupBadgeFlow
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                    }
                                                }
                                            },
                                            new FillFlowContainer
                                            {
                                                Origin = Anchor.BottomLeft,
                                                Anchor = Anchor.BottomLeft,
                                                Direction = FillDirection.Vertical,
                                                AutoSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    supporterTag = new SupporterIcon
                                                    {
                                                        Height = 20,
                                                        Margin = new MarginPadding { Top = 5 }
                                                    },
                                                    new Box
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        Height = 1.5f,
                                                        Margin = new MarginPadding { Top = 10 },
                                                        Colour = colourProvider.Light1,
                                                    },
                                                    new FillFlowContainer
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Margin = new MarginPadding { Top = 5 },
                                                        Direction = FillDirection.Horizontal,
                                                        Children = new Drawable[]
                                                        {
                                                            userFlag = new UpdateableFlag
                                                            {
                                                                Size = new Vector2(28, 20),
                                                                ShowPlaceholderOnUnknown = false,
                                                            },
                                                            userCountryText = new OsuSpriteText
                                                            {
                                                                Font = OsuFont.GetFont(size: 17.5f, weight: FontWeight.Regular),
                                                                Margin = new MarginPadding { Left = 10 },
                                                                Origin = Anchor.CentreLeft,
                                                                Anchor = Anchor.CentreLeft,
                                                                Colour = colourProvider.Light1,
                                                            }
                                                        }
                                                    },
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };

            User.BindValueChanged(user => updateUser(user.NewValue));
        }

        private void updateUser(UserProfileData? data)
        {
            var user = data?.User;

            cover.User = user;
            avatar.User = user;
            usernameText.Text = user?.Username ?? string.Empty;
            openUserExternally.Link = $@"{api.WebsiteRootUrl}/users/{user?.Id ?? 0}";
            userFlag.CountryCode = user?.CountryCode ?? default;
            userCountryText.Text = (user?.CountryCode ?? default).GetDescription();
            supporterTag.SupportLevel = user?.SupportLevel ?? 0;
            titleText.Text = user?.Title ?? string.Empty;
            titleText.Colour = Color4Extensions.FromHex(user?.Colour ?? "fff");
            groupBadgeFlow.User.Value = user;
        }

        private partial class ProfileCoverBackground : UserCoverBackground
        {
            protected override double LoadDelay => 0;

            public ProfileCoverBackground()
            {
                Masking = true;
            }
        }
    }
}
