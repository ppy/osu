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
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile
{
    public class ProfileHeader : OverlayHeader
    {
        public Bindable<User> User = new Bindable<User>();

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetUserRequest userRequest;

        private UserCoverBackground coverContainer;
        private TopHeaderContainer topHeaderContainer;
        private CentreHeaderContainer centreHeaderContainer;
        private DetailHeaderContainer detailHeaderContainer;
        private DimmedLoadingAnimation loadingAnimation;
        private ProfileRulesetSelector rulesetSelector;

        public Bindable<RulesetInfo> Ruleset
        {
            get => rulesetSelector.Current;
        }

        public ProfileHeader()
        {
            User.ValueChanged += e => updateDisplay(e.NewValue);

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
                        topHeaderContainer = new TopHeaderContainer
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
                },
                loadingAnimation = new DimmedLoadingAnimation()
            }
        };

        public void UpdateStatistics(User user)
        {
            topHeaderContainer.UpdateStatistics(user.Statistics);
            centreHeaderContainer.UpdateStatistics(user.Statistics);
            detailHeaderContainer.UpdateDisplay(user);
        }

        protected override ScreenTitle CreateTitle() => new ProfileHeaderTitle();

        private void updateDisplay(User user)
        {
            coverContainer.User = user;

            Add(rulesetSelector = new ProfileRulesetSelector
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Margin = new MarginPadding { Top = 100, Right = 30 }
            });

            rulesetSelector.SetDefaultRuleset(rulesets.GetRuleset(user.PlayMode ?? "osu"));
            rulesetSelector.SelectDefaultRuleset();
            rulesetSelector.Current.BindValueChanged(rulesetChanged);
        }

        private void rulesetChanged(ValueChangedEvent<RulesetInfo> r)
        {
            loadingAnimation.Show();

            userRequest = new GetUserRequest(User.Value.Id, r.NewValue);
            userRequest.Success += user =>
            {
                UpdateStatistics(user);
                loadingAnimation.Hide();
            };
            api.Queue(userRequest);
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
