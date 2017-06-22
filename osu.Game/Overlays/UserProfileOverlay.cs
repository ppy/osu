// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile;
using osu.Game.Users;

namespace osu.Game.Overlays
{
    public class UserProfileOverlay : WaveOverlayContainer
    {
        private ProfileSection lastSection;
        private GetUserRequest userReq;
        private APIAccess api;

        public const float CONTENT_X_MARGIN = 50;
        private const float transition_length = 500;

        public UserProfileOverlay()
        {
            FirstWaveColour = OsuColour.Gray(0.4f);
            SecondWaveColour = OsuColour.Gray(0.3f);
            ThirdWaveColour = OsuColour.Gray(0.2f);
            FourthWaveColour = OsuColour.Gray(0.1f);

            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Padding = new MarginPadding { Horizontal = 50 };
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        public void ShowUser(User user)
        {
            userReq?.Cancel();
            Clear();
            lastSection = null;

            var sections = new ProfileSection[]
            {
                new AboutSection(),
                new RecentSection(),
                new RanksSection(),
                new MedalsSection(),
                new HistoricalSection(),
                new BeatmapsSection(),
                new KudosuSection()
            };
            var tabs = new ProfileTabControl
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Height = 30
            };

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.2f)
            });

            var header = new ProfileHeader(user);

            var sectionsContainer = new SectionsContainer<ProfileSection>
            {
                RelativeSizeAxes = Axes.Both,
                ExpandableHeader = header,
                FixedHeader = tabs,
                HeaderBackground = new Box
                {
                    Colour = OsuColour.Gray(34),
                    RelativeSizeAxes = Axes.Both
                }
            };
            sectionsContainer.ScrollContainer.ScrollbarVisible = false;
            Add(sectionsContainer);
            sectionsContainer.SelectedSection.ValueChanged += s =>
            {
                if (lastSection != s)
                {
                    lastSection = s;
                    tabs.Current.Value = lastSection;
                }
            };

            tabs.Current.ValueChanged += s =>
            {
                if (lastSection == null)
                {
                    lastSection = sectionsContainer.Children.FirstOrDefault();
                    if (lastSection != null)
                        tabs.Current.Value = lastSection;
                    return;
                }
                if (lastSection != s)
                {
                    lastSection = s;
                    sectionsContainer.ScrollContainer.ScrollIntoView(lastSection);
                }
            };

            userReq = new GetUserRequest(user.Id); //fetch latest full data
            userReq.Success += u =>
            {
                header.FillFullData(u);

                for (int i = 0; i < u.ProfileOrder.Length; i++)
                {
                    string id = u.ProfileOrder[i];
                    var sec = sections.FirstOrDefault(s => s.Identifier == id);
                    if (sec != null)
                    {
                        sec.Depth = -i;
                        sectionsContainer.Add(sec);
                        tabs.AddItem(sec);
                    }
                }
            };
            api.Queue(userReq);

            Show();
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
                Add(bottom = new Box
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
                public ProfileTabItem(ProfileSection value) : base(value)
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
    }
}
