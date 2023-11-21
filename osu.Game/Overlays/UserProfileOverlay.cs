// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Rulesets;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class UserProfileOverlay : FullscreenOverlay<ProfileHeader>
    {
        protected override Container<Drawable> Content => onlineViewContainer;

        private readonly OnlineViewContainer onlineViewContainer;
        private readonly LoadingLayer loadingLayer;

        private ProfileSection? lastSection;
        private ProfileSection[]? sections;
        private GetUserRequest? userReq;
        private ProfileSectionsContainer? sectionsContainer;
        private ProfileSectionTabControl? tabs;

        private IUser? user;
        private IRulesetInfo? ruleset;

        private IBindable<APIUser> apiUser = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public UserProfileOverlay()
            : base(OverlayColourScheme.Pink)
        {
            base.Content.AddRange(new Drawable[]
            {
                onlineViewContainer = new OnlineViewContainer($"Sign in to view the {Header.Title.Title}")
                {
                    RelativeSizeAxes = Axes.Both
                },
                loadingLayer = new LoadingLayer(true)
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            apiUser = API.LocalUser.GetBoundCopy();
            apiUser.BindValueChanged(_ => Schedule(() =>
            {
                if (API.IsLoggedIn)
                    fetchAndSetContent();
            }));
        }

        protected override ProfileHeader CreateHeader() => new ProfileHeader();

        protected override Color4 BackgroundColour => ColourProvider.Background5;

        public void ShowUser(IUser userToShow, IRulesetInfo? userRuleset = null)
        {
            if (userToShow.OnlineID == APIUser.SYSTEM_USER_ID)
                return;

            user = userToShow;
            ruleset = userRuleset;

            Show();

            fetchAndSetContent();
        }

        private void fetchAndSetContent()
        {
            Debug.Assert(user != null);

            if (user.OnlineID == Header.User.Value?.User.Id && ruleset?.MatchesOnlineID(Header.User.Value?.Ruleset) == true)
                return;

            if (sectionsContainer != null)
                sectionsContainer.ExpandableHeader = null;

            userReq?.Cancel();
            Clear();
            lastSection = null;

            sections = !user.IsBot
                ? new ProfileSection[]
                {
                    //new AboutSection(),
                    new RecentSection(),
                    new RanksSection(),
                    //new MedalsSection(),
                    new HistoricalSection(),
                    new BeatmapsSection(),
                    new KudosuSection()
                }
                : Array.Empty<ProfileSection>();

            tabs = new ProfileSectionTabControl
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            };

            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = sectionsContainer = new ProfileSectionsContainer
                {
                    ExpandableHeader = Header,
                    FixedHeader = tabs,
                    HeaderBackground = new Box
                    {
                        // this is only visible as the ProfileTabControl background
                        Colour = ColourProvider.Background5,
                        RelativeSizeAxes = Axes.Both
                    },
                }
            });

            sectionsContainer.SelectedSection.ValueChanged += section =>
            {
                if (lastSection != section.NewValue)
                {
                    lastSection = section.NewValue;
                    tabs.Current.Value = lastSection!;
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

            sectionsContainer.ScrollToTop();

            if (!API.IsLoggedIn)
                return;

            userReq = user.OnlineID > 1 ? new GetUserRequest(user.OnlineID, ruleset) : new GetUserRequest(user.Username, ruleset);
            userReq.Success += u => userLoadComplete(u, ruleset);
            API.Queue(userReq);
            loadingLayer.Show();
        }

        private void userLoadComplete(APIUser loadedUser, IRulesetInfo? userRuleset)
        {
            Debug.Assert(sections != null && sectionsContainer != null && tabs != null);

            var actualRuleset = rulesets.GetRuleset(userRuleset?.ShortName ?? loadedUser.PlayMode).AsNonNull();

            var userProfile = new UserProfileData(loadedUser, actualRuleset);
            Header.User.Value = userProfile;

            if (loadedUser.ProfileOrder != null)
            {
                foreach (string id in loadedUser.ProfileOrder)
                {
                    var sec = sections.FirstOrDefault(s => s.Identifier == id);

                    if (sec != null)
                    {
                        sec.User.Value = userProfile;

                        sectionsContainer.Add(sec);
                        tabs.AddItem(sec);
                    }
                }
            }

            loadingLayer.Hide();
        }

        private partial class ProfileSectionTabControl : OsuTabControl<ProfileSection>
        {
            public ProfileSectionTabControl()
            {
                Height = 40;
                Padding = new MarginPadding { Horizontal = HORIZONTAL_PADDING };
                TabContainer.Spacing = new Vector2(20);
            }

            protected override TabItem<ProfileSection> CreateTabItem(ProfileSection value) => new ProfileSectionTabItem(value);

            protected override bool OnClick(ClickEvent e) => true;

            protected override bool OnHover(HoverEvent e) => true;

            private partial class ProfileSectionTabItem : TabItem<ProfileSection>
            {
                private OsuSpriteText text = null!;

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                public ProfileSectionTabItem(ProfileSection value)
                    : base(value)
                {
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    AutoSizeAxes = Axes.Both;
                    Anchor = Anchor.CentreLeft;
                    Origin = Anchor.CentreLeft;

                    InternalChild = text = new OsuSpriteText
                    {
                        Text = Value.Title
                    };

                    updateState();
                }

                protected override void OnActivated() => updateState();

                protected override void OnDeactivated() => updateState();

                protected override bool OnHover(HoverEvent e)
                {
                    updateState();
                    return true;
                }

                protected override void OnHoverLost(HoverLostEvent e) => updateState();

                private void updateState()
                {
                    text.Font = OsuFont.Default.With(size: 14, weight: Active.Value ? FontWeight.SemiBold : FontWeight.Regular);

                    Colour4 textColour;

                    if (IsHovered)
                        textColour = colourProvider.Light1;
                    else
                        textColour = Active.Value ? colourProvider.Content1 : colourProvider.Light2;

                    text.FadeColour(textColour, 300, Easing.OutQuint);
                }
            }
        }

        private partial class ProfileSectionsContainer : SectionsContainer<ProfileSection>
        {
            private OverlayScrollContainer scroll = null!;

            public ProfileSectionsContainer()
            {
                RelativeSizeAxes = Axes.Both;
            }

            protected override UserTrackingScrollContainer CreateScrollContainer() => scroll = new OverlayScrollContainer();

            // Reverse child ID is required so expanding beatmap panels can appear above sections below them.
            // This can also be done by setting Depth when adding new sections above if using ReverseChildID turns out to have any issues.
            protected override FlowContainer<ProfileSection> CreateScrollContentContainer() => new ReverseChildIDFillFlowContainer<ProfileSection>
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Spacing = new Vector2(0, 10),
                Padding = new MarginPadding { Horizontal = 10 },
                Margin = new MarginPadding { Bottom = 10 },
            };

            protected override void LoadComplete()
            {
                base.LoadComplete();

                // Ensure the scroll-to-top button is displayed above the fixed header.
                AddInternal(scroll.Button.CreateProxy());
            }
        }
    }
}
