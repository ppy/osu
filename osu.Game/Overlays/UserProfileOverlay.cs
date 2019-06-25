// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays
{
    public class UserProfileOverlay : FullscreenOverlay
    {
        private GetUserRequest userReq;
        protected ProfileHeader Header;
        private ProfileSectionsContainer sectionsContainer;
        private ProfileTabControl tabs;

        private CancellationTokenSource cancellationSource;

        public const float CONTENT_X_MARGIN = 70;

        public UserProfileOverlay()
        {
            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.1f)
            });
        }

        public void ShowUser(long userId) => ShowUser(new User { Id = userId });

        public void ShowUser(User user)
        {
            cancellationSource?.Cancel();
            cancellationSource = null;

            userReq?.Cancel();
            userReq = null;

            if (user == User.SYSTEM_USER) return;

            Show();

            // we are already showing the correct user.
            if (user.Id == Header?.User.Value?.Id)
                return;

            // temporary until this class supports proper bindable flow (like BeatmapSetOverlay).
            sectionsContainer?.FadeOut(200);
            sectionsContainer?.Expire();

            userReq = new GetUserRequest(user.Id);
            userReq.Success += userLoadComplete;
            API.Queue(userReq);
        }

        private void userLoadComplete(User user)
        {
            // temporary until this class supports proper bindable flow (like BeatmapSetOverlay).
            sectionsContainer = new ProfileSectionsContainer
            {
                ExpandableHeader = Header = new ProfileHeader { User = { Value = user } },
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
                }
            };

            ProfileSection lastSection = null;

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

            if (user.ProfileOrder != null)
            {
                var sections = new ProfileSection[]
                {
                    //new AboutSection(),
                    new RecentSection(),
                    new RanksSection(),
                    //new MedalsSection(),
                    new HistoricalSection(),
                    new BeatmapsSection(),
                    new KudosuSection()
                };

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

            LoadComponentAsync(sectionsContainer, Add, (cancellationSource = new CancellationTokenSource()).Token);
        }

        private class ProfileTabControl : PageTabControl<ProfileSection>
        {
            private readonly Box bottom;

            public ProfileTabControl()
            {
                TabContainer.RelativeSizeAxes &= ~Axes.X;
                TabContainer.AutoSizeAxes |= Axes.X;
                TabContainer.Anchor |= Anchor.x1;
                TabContainer.Origin |= Anchor.x1;
                AddInternal(bottom = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    EdgeSmoothness = new Vector2(1)
                });
            }

            protected override TabItem<ProfileSection> CreateTabItem(ProfileSection value) => new ProfileTabItem(value);

            protected override Dropdown<ProfileSection> CreateDropdown() => null;

            private class ProfileTabItem : PageTabItem
            {
                public ProfileTabItem(ProfileSection value)
                    : base(value)
                {
                    Text.Text = value.Title;
                }
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                bottom.Colour = colours.Yellow;
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
