// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Users;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Profile
{
    public class ProfileHeader : Container
    {
        private readonly UserCoverBackground coverContainer;
        private readonly ProfileHeaderTabControl infoTabControl;

        private const float cover_height = 150;
        private const float cover_info_height = 75;

        public ProfileHeader()
        {
            CenterHeaderContainer centerHeaderContainer;
            DetailHeaderContainer detailHeaderContainer;
            Container expandedDetailContainer;
            FillFlowContainer hiddenDetailContainer, headerDetailContainer;
            SpriteIcon expandButtonIcon;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                coverContainer = new UserCoverBackground
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                },
                new Container
                {
                    Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                    Y = cover_height,
                    Height = cover_info_height,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.BottomLeft,
                    Depth = -float.MaxValue,
                    Children = new Drawable[]
                    {
                        new ProfileHeaderTitle
                        {
                            X = -ScreenTitle.ICON_WIDTH,
                        },
                        infoTabControl = new ProfileHeaderTabControl
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = cover_info_height - 30,
                            Margin = new MarginPadding { Left = -UserProfileOverlay.CONTENT_X_MARGIN },
                            Padding = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN }
                        }
                    }
                },
                new FillFlowContainer
                {
                    Margin = new MarginPadding { Top = cover_height },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new TopHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            User = { BindTarget = User },
                        },
                        centerHeaderContainer = new CenterHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            User = { BindTarget = User },
                        },
                        detailHeaderContainer = new DetailHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            User = { BindTarget = User },
                        },
                        new MedalHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            User = { BindTarget = User },
                        },
                        new BottomHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            User = { BindTarget = User },
                        },
                    }
                }
            };

            infoTabControl.AddItem("Info");
            infoTabControl.AddItem("Modding");

            centerHeaderContainer.DetailsVisibilityAction = visible => detailHeaderContainer.Alpha = visible ? 0 : 1;
            User.ValueChanged += e => updateDisplay(e.NewValue);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            infoTabControl.AccentColour = colours.CommunityUserGreen;
        }

        public Bindable<User> User = new Bindable<User>();

        private void updateDisplay(User user)
        {
            coverContainer.User = user;
        }

        public class HasTooltipContainer : Container, IHasTooltip
        {
            public string TooltipText { get; set; }
        }

        private class ProfileHeaderTitle : ScreenTitle
        {
            public ProfileHeaderTitle()
            {
                Title = "Player ";
                Section = "Info";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.CommunityUserGreen;
            }
        }
    }
}
