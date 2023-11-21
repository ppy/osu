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
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
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
        private const float content_height = 65;
        private const float vertical_padding = 10;

        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RankingsOverlay? rankingsOverlay { get; set; }

        private UserCoverBackground cover = null!;
        private SupporterIcon supporterTag = null!;
        private UpdateableAvatar avatar = null!;
        private OsuSpriteText usernameText = null!;
        private ExternalLinkButton openUserExternally = null!;
        private OsuSpriteText titleText = null!;
        private ClickableUpdateableFlag userFlag = null!;
        private OsuHoverContainer userCountryContainer = null!;
        private OsuSpriteText userCountryText = null!;
        private GroupBadgeFlow groupBadgeFlow = null!;
        private ToggleCoverButton coverToggle = null!;
        private PreviousUsernamesDisplay previousUsernamesDisplay = null!;

        private Bindable<bool> coverExpanded = null!;

        private FillFlowContainer flow = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuConfigManager configManager)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            coverExpanded = configManager.GetBindable<bool>(OsuSetting.ProfileCoverExpanded);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
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
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                flow = new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    Padding = new MarginPadding
                                    {
                                        Left = WaveOverlayContainer.HORIZONTAL_PADDING,
                                        Vertical = vertical_padding
                                    },
                                    Height = content_height + 2 * vertical_padding,
                                    RelativeSizeAxes = Axes.X,
                                    Children = new Drawable[]
                                    {
                                        avatar = new UpdateableAvatar(isInteractive: false, showGuestOnNull: false)
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Masking = true,
                                            EdgeEffect = new EdgeEffectParameters
                                            {
                                                Type = EdgeEffectType.Shadow,
                                                Offset = new Vector2(0, 1),
                                                Radius = 3,
                                                Colour = Colour4.Black.Opacity(0.25f),
                                            }
                                        },
                                        new FillFlowContainer
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
                                                        new Container
                                                        {
                                                            // Intentionally use a zero-size container, else the fill flow will adjust to (and cancel) the upwards animation.
                                                            Child = previousUsernamesDisplay = new PreviousUsernamesDisplay(),
                                                        }
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
                                                        userFlag = new ClickableUpdateableFlag
                                                        {
                                                            Size = new Vector2(28, 20),
                                                            ShowPlaceholderOnUnknown = false,
                                                        },
                                                        userCountryContainer = new OsuHoverContainer
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Margin = new MarginPadding { Left = 5 },
                                                            Child = userCountryText = new OsuSpriteText
                                                            {
                                                                Font = OsuFont.GetFont(size: 14f, weight: FontWeight.Regular),
                                                            },
                                                        },
                                                    }
                                                },
                                            }
                                        },
                                    }
                                },
                                coverToggle = new ToggleCoverButton
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Margin = new MarginPadding { Right = 10 },
                                    CoverExpanded = { BindTarget = coverExpanded }
                                }
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(user => updateUser(user.NewValue), true);
            coverExpanded.BindValueChanged(_ => updateCoverState(), true);
            FinishTransforms(true);
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
            userCountryContainer.Action = () => rankingsOverlay?.ShowCountry(user?.CountryCode ?? default);
            supporterTag.SupportLevel = user?.SupportLevel ?? 0;
            titleText.Text = user?.Title ?? string.Empty;
            titleText.Colour = Color4Extensions.FromHex(user?.Colour ?? "fff");
            groupBadgeFlow.User.Value = user;
            previousUsernamesDisplay.User.Value = user;
        }

        private void updateCoverState()
        {
            const float transition_duration = 500;

            bool expanded = coverToggle.CoverExpanded.Value;

            cover.ResizeHeightTo(expanded ? 250 : 0, transition_duration, Easing.OutQuint);
            avatar.ResizeTo(new Vector2(expanded ? 120 : content_height), transition_duration, Easing.OutQuint);
            avatar.TransformTo(nameof(avatar.CornerRadius), expanded ? 40f : 20f, transition_duration, Easing.OutQuint);
            flow.TransformTo(nameof(flow.Spacing), new Vector2(expanded ? 20f : 10f), transition_duration, Easing.OutQuint);
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
