// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays
{
    public class UserProfileOverlay : FullscreenOverlay
    {
        private ProfileSection lastSection;
        private ProfileSection[] sections;
        private GetUserRequest userReq;
        protected ProfileHeader Header;
        private ProfileSectionsContainer sectionsContainer;
        private ProfileTabControl tabs;

        public const float CONTENT_X_MARGIN = 70;

        public void ShowUser(long userId) => ShowUser(new User { Id = userId });

        public void ShowUser(User user, bool fetchOnline = true)
        {
            if (user == User.SYSTEM_USER)
                return;

            Show();

            if (user.Id == Header?.User.Value?.Id)
                return;

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
                : new ProfileSection[]
                {
                    //new AboutSection(),
                };

            tabs = new ProfileTabControl
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Height = 30
            };

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.1f)
            });

            Add(sectionsContainer = new ProfileSectionsContainer
            {
                ExpandableHeader = Header = new ProfileHeader(),
                FixedHeader = tabs,
                HeaderBackground = new Box
                {
                    Colour = OsuColour.Gray(34),
                    RelativeSizeAxes = Axes.Both
                },
            });
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

            if (fetchOnline)
            {
                userReq = new GetUserRequest(user.Id);
                userReq.Success += userLoadComplete;
                API.Queue(userReq);
            }
            else
            {
                userReq = null;
                userLoadComplete(user);
            }

            sectionsContainer.ScrollToTop();
        }

        private void userLoadComplete(User user)
        {
            Header.User.Value = user;

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
