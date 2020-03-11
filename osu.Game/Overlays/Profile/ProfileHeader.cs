// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile
{
    public class ProfileHeader : TabControlOverlayHeader<string>
    {
        private UserCoverBackground coverContainer;

        public Bindable<User> User = new Bindable<User>();

        private CentreHeaderContainer centreHeaderContainer;
        private DetailHeaderContainer detailHeaderContainer;

        public ProfileHeader()
        {
            User.ValueChanged += e => updateDisplay(e.NewValue);

            TabControl.AddItem("info");
            TabControl.AddItem("modding");

            centreHeaderContainer.DetailsVisible.BindValueChanged(visible => detailHeaderContainer.Expanded = visible.NewValue, true);
        }

        protected override Drawable CreateBackground() =>
            new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 150,
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
                        Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("222").Opacity(0.8f), Color4Extensions.FromHex("222").Opacity(0.2f))
                    },
                }
            };

        protected override Drawable CreateContent() => new FillFlowContainer
        {
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
        };

        protected override ScreenTitle CreateTitle() => new ProfileHeaderTitle();

        private void updateDisplay(User user) => coverContainer.User = user;

        private class ProfileHeaderTitle : ScreenTitle
        {
            public ProfileHeaderTitle()
            {
                Title = "player";
                Section = "info";
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/profile");
        }
    }
}
