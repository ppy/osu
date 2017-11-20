// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Users;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays
{
    public class UserProfileOverlay : WaveOverlayContainer
    {
        private ProfileSection lastSection;
        private ProfileSection[] sections;
        private GetUserRequest userReq;
        private GetUserRequest modeReq;
        private APIAccess api;
        private Container header;
        private ProfileHeader profileHeader;
        private SectionsContainer<ProfileSection> sectionsContainer;
        private ProfileTabControl sectionTabs;
        private ModeTabControl modeTabs;

        public const float CONTENT_X_MARGIN = 50;

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        protected override bool OnClick(InputState state)
        {
            State = Visibility.Hidden;
            return true;
        }

        public UserProfileOverlay()
        {
            FirstWaveColour = OsuColour.Gray(0.4f);
            SecondWaveColour = OsuColour.Gray(0.3f);
            ThirdWaveColour = OsuColour.Gray(0.2f);
            FourthWaveColour = OsuColour.Gray(0.1f);

            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Width = 0.85f;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0),
                Type = EdgeEffectType.Shadow,
                Radius = 10
            };
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        protected override void PopIn()
        {
            base.PopIn();
            FadeEdgeEffectTo(0.5f, APPEAR_DURATION, Easing.In);
        }

        protected override void PopOut()
        {
            base.PopOut();
            FadeEdgeEffectTo(0, DISAPPEAR_DURATION, Easing.Out);
        }

        public void ShowUser(User user, bool fetchOnline = true)
        {
            userReq?.Cancel();
            Clear();
            lastSection = null;

            sections = new ProfileSection[]
            {
                //new AboutSection(),
                //new RecentSection(),
                new RanksSection(),
                //new MedalsSection(),
                new HistoricalSection(),
                new BeatmapsSection(),
                //new KudosuSection()
            };
            sectionTabs = new ProfileTabControl
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

            header = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    modeTabs = new ModeTabControl
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Height = 35,
                        Alpha = 0,
                        AlwaysPresent = true,
                    },
                    profileHeader = new ProfileHeader(user)
                    {
                        Margin = new MarginPadding{ Top = 35 },
                    }
                }
            };

            profileHeader.IsReloading = true;

            modeTabs.AddItem("osu!");
            modeTabs.AddItem("osu!taiko");
            modeTabs.AddItem("osu!catch");
            modeTabs.AddItem("osu!mania");

            Add(sectionsContainer = new SectionsContainer<ProfileSection>
            {
                RelativeSizeAxes = Axes.Both,
                ExpandableHeader = header,
                FixedHeader = sectionTabs,
                HeaderBackground = new Box
                {
                    Colour = OsuColour.Gray(34),
                    RelativeSizeAxes = Axes.Both
                }
            });
            sectionsContainer.SelectedSection.ValueChanged += s =>
            {
                if (lastSection != s)
                {
                    lastSection = s;
                    sectionTabs.Current.Value = lastSection;
                }
            };

            sectionTabs.Current.ValueChanged += s =>
            {
                if (lastSection == null)
                {
                    lastSection = sectionsContainer.Children.FirstOrDefault();
                    if (lastSection != null)
                        sectionTabs.Current.Value = lastSection;
                    return;
                }
                if (lastSection != s)
                {
                    lastSection = s;
                    sectionsContainer.ScrollTo(lastSection);
                }
            };

            if (fetchOnline)
            {
                userReq = new GetUserRequest(user.Id);
                userReq.Success += userLoadComplete;
                api.Queue(userReq);
            }
            else
            {
                userReq = null;
                userLoadComplete(user);
            }

            Show();
            sectionsContainer.ScrollToTop();
        }

        private void userLoadComplete(User user)
        {
            profileHeader.User = user;

            foreach (string id in user.ProfileOrder)
            {
                var sec = sections.FirstOrDefault(s => s.Identifier == id);
                if (sec != null)
                {
                    sec.User.Value = user;

                    sectionsContainer.Add(sec);
                    sectionTabs.AddItem(sec);
                }
            }

            string playMode = "";
            switch (user.PlayMode)
            {
                case "osu":
                    playMode = "osu!";
                    break;
                case "mania":
                    playMode = "osu!mania";
                    break;
                case "fruits":
                    playMode = "osu!catch";
                    break;
                case "taiko":
                    playMode = "osu!taiko";
                    break;
            }
            modeTabs.Current.Value = playMode;
            modeTabs.FadeIn(200, Easing.Out);

            modeTabs.Current.ValueChanged += updateMode;
        }

        private void updateMode(string newMode)
        {
            modeReq?.Cancel();

            foreach (var s in sections)
                s.PlayMode = getModeFromString(newMode);

            profileHeader.IsReloading = true;

            modeReq = new GetUserRequest(profileHeader.User.Id, getModeFromString(newMode));
            modeReq.Success += user => profileHeader.User = user;
            api.Queue(modeReq);
        }

        private Mode getModeFromString(string mode)
        {
            switch (mode)
            {
                case "osu!":
                    return Mode.Osu;
                case "osu!mania":
                    return Mode.Mania;
                case "osu!catch":
                    return Mode.Fruits;
                case "osu!taiko":
                    return Mode.Taiko;
            }

            return Mode.Default;
        }

        private class UserTabControl<T> : PageTabControl<T>
        {
            protected override Dropdown<T> CreateDropdown() => null;

            protected override bool OnClick(InputState state) => true;

            private readonly Box bottom;

            protected UserTabControl()
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

                TabContainer.Spacing = new Vector2(20, 0);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                bottom.Colour = colours.Yellow;
            }

            protected class UserTabItem : PageTabItem
            {
                private const int fade_duration = 100;

                protected readonly SpriteText TextRegular;

                protected UserTabItem(T value) : base(value)
                {
                    Add(TextRegular = new OsuSpriteText
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding { Bottom = 7 },
                        Font = @"Exo2.0-Regular",
                    });

                    Text.Alpha = 0;
                    Text.AlwaysPresent = true;
                    Text.TextSize = TextRegular.TextSize = 20;
                }

                protected override void OnActivated()
                {
                    base.OnActivated();
                    TextRegular.FadeOut(fade_duration, Easing.Out);
                    Text.FadeIn(fade_duration, Easing.Out);
                }

                protected override void OnDeactivated()
                {
                    base.OnDeactivated();
                    TextRegular.FadeIn(fade_duration, Easing.Out);
                    Text.FadeOut(fade_duration, Easing.Out);
                }
            }
        }

        private class ProfileTabControl : UserTabControl<ProfileSection>
        {
            protected override TabItem<ProfileSection> CreateTabItem(ProfileSection value) => new ProfileTabItem(value);

            private class ProfileTabItem : UserTabItem
            {
                public ProfileTabItem(ProfileSection value) : base(value)
                {
                    Text.Text = TextRegular.Text = value.Title;
                }
            }
        }

        private class ModeTabControl : UserTabControl<string>
        {
            protected override TabItem<string> CreateTabItem(string value) => new ModeTabItem(value);

            private class ModeTabItem : UserTabItem
            {
                public ModeTabItem(string value) : base(value)
                {
                    Text.Text = TextRegular.Text = value;
                }
            }
        }
    }
}
