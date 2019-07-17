// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Rulesets;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays
{
    public class UserProfileOverlay : FullscreenOverlay
    {
        public const float CONTENT_X_MARGIN = 70;

        protected ProfileHeader Header;

        private ProfileSection lastSection;
        private ProfileSection[] sections;
        private GetUserRequest userReq;
        private ProfileSectionsContainer sectionsContainer;
        private ProfileTabControl tabs;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private readonly Bindable<User> displayedUser = new Bindable<User>();
        private readonly Bindable<RulesetInfo> displayedRuleset = new Bindable<RulesetInfo>();

        private readonly Bindable<UserStatistics> statistics = new Bindable<UserStatistics>();

        [BackgroundDependencyLoader]
        private void load()
        {
            sections = new ProfileSection[]
            {
                //new AboutSection(),
                new RecentSection(),
                new RanksSection(),
                //new MedalsSection(),
                new HistoricalSection(),
                new BeatmapsSection(),
                new KudosuSection()
            };

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.1f)
                },
                sectionsContainer = new ProfileSectionsContainer
                {
                    ExpandableHeader = Header = new ProfileHeader
                    {
                        User = { BindTarget = displayedUser },
                        Statistics = { BindTarget = statistics },
                    },
                    FixedHeader = tabs = new ProfileTabControl
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Height = 30
                    },
                    HeaderBackground = new Box
                    {
                        Colour = OsuColour.Gray(34),
                        RelativeSizeAxes = Axes.Both
                    },
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var sec in sections)
            {
                sec.Ruleset.BindTo(displayedRuleset);
                sec.User.BindTo(displayedUser);
            }

            sectionsContainer.SelectedSection.ValueChanged += section =>
            {
                if (lastSection != section.NewValue)
                {
                    lastSection = section.NewValue;
                    tabs.Current.Value = lastSection;
                }
            };

            tabs.Current.ValueChanged += section =>
            {
                if (lastSection == null)
                {
                    lastSection = sectionsContainer.Children.FirstOrDefault();
                    if (lastSection != null)
                        tabs.Current.Value = lastSection;
                    return;
                }

                if (lastSection != section.NewValue)
                {
                    lastSection = section.NewValue;
                    sectionsContainer.ScrollTo(lastSection);
                }
            };

            displayedRuleset.ValueChanged += r =>
            {
                if (Header != null)
                    Header.Ruleset.Value = r.NewValue;
            };

            displayedUser.ValueChanged += u =>
            {
                lastSection = null;

                sectionsContainer.Clear(false);
                tabs.Clear();

                var user = u.NewValue;

                if (user == null) return;

                if (user.ProfileOrder != null)
                {
                    foreach (string id in user.ProfileOrder)
                    {
                        var sec = sections.FirstOrDefault(s => s.Identifier == id);

                        if (sec != null)
                        {
                            sec.User.Value = user;

                            sectionsContainer.Add(sec);
                            tabs.AddItem(sec);
                        }
                    }
                }
            };

            Header.Ruleset.ValueChanged += headerRulesetChanged;
        }

        public void ShowUser(long userId) => ShowUser(new User { Id = userId });

        public void ShowUser(User user, bool fetchOnline = true)
        {
            if (user == User.SYSTEM_USER)
                return;

            Show();

            if (user.Id == displayedUser.Value?.Id)
                return;

            if (fetchOnline)
                fetchOnlineUser(user.Id);
            else
                userLoadComplete(user);

            sectionsContainer.ScrollToTop();
        }

        private void fetchOnlineUser(long userId, RulesetInfo ruleset = null)
        {
            Header.Loading = true;

            userReq?.Cancel();

            if (userId != displayedUser.Value?.Id)
                displayedUser.Value = null;
            displayedRuleset.Value = ruleset;

            userReq = new GetUserRequest(userId, ruleset);
            userReq.Success += userLoadComplete;
            API.Queue(userReq);
        }

        private void userLoadComplete(User user)
        {
            displayedUser.Value = user;
            if (displayedRuleset.Value == null)
                displayedRuleset.Value = user.GetDefaultRuleset(rulesets);
            statistics.Value = user.Statistics;

            Header.Loading = false;
        }

        private void headerRulesetChanged(ValueChangedEvent<RulesetInfo> ruleset)
        {
            if (displayedUser.Value == null || ruleset.NewValue == null) return;

            // avoid feedback from ruleset selector on initial update.
            if (ruleset.NewValue.Equals(displayedRuleset.Value)) return;

            fetchOnlineUser(displayedUser.Value.Id, ruleset.NewValue);
        }

        private class ProfileTabControl : OverlayTabControl<ProfileSection>
        {
            public ProfileTabControl()
            {
                TabContainer.RelativeSizeAxes &= ~Axes.X;
                TabContainer.AutoSizeAxes |= Axes.X;
                TabContainer.Anchor |= Anchor.x1;
                TabContainer.Origin |= Anchor.x1;
            }

            protected override TabItem<ProfileSection> CreateTabItem(ProfileSection value) => new ProfileTabItem(value)
            {
                AccentColour = AccentColour
            };

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Seafoam;
            }

            private class ProfileTabItem : OverlayTabItem<ProfileSection>
            {
                public ProfileTabItem(ProfileSection value)
                    : base(value)
                {
                    Text.Text = value.Title;
                }
            }
        }

        private class ProfileSectionsContainer : SectionsContainer<ProfileSection>
        {
            public ProfileSectionsContainer()
            {
                RelativeSizeAxes = Axes.Both;
            }

            protected override FlowContainer<ProfileSection> CreateScrollContentContainer() => new FillFlowContainer<ProfileSection>
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Spacing = new Vector2(0, 20),
            };
        }
    }
}
