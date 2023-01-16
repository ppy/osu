﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile
{
    public partial class ProfileHeader : TabControlOverlayHeader<LocalisableString>
    {
        private UserCoverBackground coverContainer = null!;

        public Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        private CentreHeaderContainer centreHeaderContainer;
        private DetailHeaderContainer detailHeaderContainer;

        public ProfileHeader()
        {
            ContentSidePadding = UserProfileOverlay.CONTENT_X_MARGIN;

            User.ValueChanged += e => updateDisplay(e.NewValue);

            TabControl.AddItem(LayoutStrings.HeaderUsersShow);

            // todo: pending implementation.
            // TabControl.AddItem(LayoutStrings.HeaderUsersModding);

            // Haphazardly guaranteed by OverlayHeader constructor (see CreateBackground / CreateContent).
            Debug.Assert(centreHeaderContainer != null);
            Debug.Assert(detailHeaderContainer != null);
        }

        protected override Drawable CreateBackground() =>
            new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 150,
                Masking = true,
                Children = new Drawable[]
                {
                    coverContainer = new ProfileCoverBackground
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
                new MedalHeaderContainer
                {
                    RelativeSizeAxes = Axes.X,
                    User = { BindTarget = User },
                },
                detailHeaderContainer = new DetailHeaderContainer
                {
                    RelativeSizeAxes = Axes.X,
                    User = { BindTarget = User },
                },
                centreHeaderContainer = new CentreHeaderContainer
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

        protected override OverlayTitle CreateTitle() => new ProfileHeaderTitle();

        protected override Drawable CreateTabControlContent() => new ProfileRulesetSelector
        {
            User = { BindTarget = User }
        };

        private void updateDisplay(UserProfileData? user) => coverContainer.User = user?.User;

        private partial class ProfileHeaderTitle : OverlayTitle
        {
            public ProfileHeaderTitle()
            {
                Title = PageTitleStrings.MainUsersControllerDefault;
                IconTexture = "Icons/Hexacons/profile";
            }
        }

        private partial class ProfileCoverBackground : UserCoverBackground
        {
            protected override double LoadDelay => 0;
        }
    }
}
