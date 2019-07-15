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
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile
{
    public class ProfileHeader : OverlayHeader
    {
        private UserCoverBackground coverContainer;
        private CentreHeaderContainer centreHeaderContainer;
        private DetailHeaderContainer detailHeaderContainer;
        private DimmedLoadingAnimation loadingAnimation;
        private readonly ProfileRulesetSelector rulesetSelector;

        public Bindable<User> User = new Bindable<User>();
        public Bindable<UserStatistics> Statistics = new Bindable<UserStatistics>();

        public Bindable<RulesetInfo> Ruleset => rulesetSelector.Current;

        private bool loading;

        public bool Loading
        {
            get => loading;
            set
            {
                if (loading == value)
                    return;

                loading = value;

                if (loading)
                    loadingAnimation.Show();
                else
                    loadingAnimation.Hide();
            }
        }

        public ProfileHeader()
        {
            Add(rulesetSelector = new ProfileRulesetSelector
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Margin = new MarginPadding { Top = 100, Right = 30 },
                User = { BindTarget = User }
            });

            User.BindValueChanged(userChanged);

            TabControl.AddItem("Info");
            TabControl.AddItem("Modding");

            centreHeaderContainer.DetailsVisible.BindValueChanged(visible => detailHeaderContainer.Expanded = visible.NewValue, true);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            TabControl.AccentColour = colours.Seafoam;
        }

        protected override Drawable CreateBackground() =>
            new Container
            {
                RelativeSizeAxes = Axes.Both,
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
            };

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new FillFlowContainer
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
                            Statistics = { BindTarget = Statistics },
                        },
                        centreHeaderContainer = new CentreHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            User = { BindTarget = User },
                            Statistics = { BindTarget = Statistics },
                        },
                        detailHeaderContainer = new DetailHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            User = { BindTarget = User },
                            Statistics = { BindTarget = Statistics },
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
                },
                loadingAnimation = new DimmedLoadingAnimation()
            }
        };

        protected override ScreenTitle CreateTitle() => new ProfileHeaderTitle();

        private void userChanged(ValueChangedEvent<User> user)
        {
            coverContainer.User = user.NewValue;
            Statistics.Value = user.NewValue?.Statistics;
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
                AccentColour = colours.Seafoam;
            }
        }
    }
}
