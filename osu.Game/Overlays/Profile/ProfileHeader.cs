// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Users;

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
            CentreHeaderContainer centreHeaderContainer;
            DetailHeaderContainer detailHeaderContainer;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        coverContainer = new UserCoverBackground
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(OsuColour.FromHex("222").Opacity(0.8f), OsuColour.FromHex("222").Opacity(0.2f))
                        },
                    }
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
                        centreHeaderContainer = new CentreHeaderContainer
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

            centreHeaderContainer.DetailsVisible.BindValueChanged(visible => detailHeaderContainer.Alpha = visible.NewValue ? 1 : 0, true);
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

        private class ProfileHeaderTitle : ScreenTitle
        {
            public ProfileHeaderTitle()
            {
                Title = "Player";
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
