// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
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
        private const float avatar_size = 120;

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
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    Padding = new MarginPadding
                                    {
                                        Left = UserProfileOverlay.CONTENT_X_MARGIN,
                                        Vertical = 10
                                    },
                                    Spacing = new Vector2(20, 0),
                                    Height = 85,
                                    RelativeSizeAxes = Axes.X,
                                    Children = new Drawable[]
                                    {
                                        avatar = new UpdateableAvatar(isInteractive: false, showGuestOnNull: false)
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Size = new Vector2(avatar_size),
                                            Masking = true,
                                            CornerRadius = avatar_size * 0.25f,
                                            EdgeEffect = new EdgeEffectParameters
                                            {
                                                Type = EdgeEffectType.Shadow,
                                                Offset = new Vector2(0, 1),
                                                Radius = 3,
                                                Colour = Colour4.Black.Opacity(0.25f),
                                            }
                                        },
                                        new OsuContextMenuContainer
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            RelativeSizeAxes = Axes.Y,
                                            AutoSizeAxes = Axes.X,
                                            Child = new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Vertical,
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Children = new Drawable[]
                                                {
                                                    new FillFlowContainer
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Direction = FillDirection.Horizontal,
                                                        Spacing = new Vector2(5, 0),
                                                        Children = new Drawable[]
                                                        {
                                                            usernameText = new OsuSpriteText
                                                            {
                                                                Font = OsuFont.GetFont(size: 24, weight: FontWeight.Regular)
                                                            },
                                                            supporterTag = new SupporterIcon
                                                            {
                                                                Anchor = Anchor.CentreLeft,
                                                                Origin = Anchor.CentreLeft,
                                                                Height = 15,
                                                            },
                                                            openUserExternally = new ExternalLinkButton
                                                            {
                                                                Anchor = Anchor.CentreLeft,
                                                                Origin = Anchor.CentreLeft,
                                                            },
                                                            groupBadgeFlow = new GroupBadgeFlow
                                                            {
                                                                Anchor = Anchor.CentreLeft,
                                                                Origin = Anchor.CentreLeft,
                                                            },
                                                        }
                                                    },
                                                    titleText = new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 16, weight: FontWeight.Regular),
                                                        Margin = new MarginPadding { Bottom = 5 }
                                                    },
                                                    new FillFlowContainer
                                                    {
                                                        AutoSizeAxes = Axes.Both,
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
                                                                Font = OsuFont.GetFont(size: 14f, weight: FontWeight.Regular),
                                                                Margin = new MarginPadding { Left = 5 },
                                                                Origin = Anchor.CentreLeft,
                                                                Anchor = Anchor.CentreLeft,
                                                            }
                                                        }
                                                    },
                                                }
                                            },
                                        },
                                    }
                                },
                                new ToggleCoverButton
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Margin = new MarginPadding { Right = 10 }
                                }
                            },
                        },
                    },
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
