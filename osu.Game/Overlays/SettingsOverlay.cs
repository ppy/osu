// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;

namespace osu.Game.Overlays
{
    public class SettingsOverlay : OsuFocusedOverlayContainer
    {
        internal const float CONTENT_MARGINS = 10;

        public const float TRANSITION_LENGTH = 600;

        public const float SIDEBAR_WIDTH = Sidebar.DEFAULT_WIDTH;

        private const float width = 400;

        private const float sidebar_padding = 10;

        private Sidebar sidebar;
        private SidebarButton[] sidebarButtons;
        private SidebarButton selectedSidebarButton;

        private SettingsSectionsContainer sectionsContainer;

        private SearchTextBox searchTextBox;

        public SettingsOverlay()
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGame game)
        {
            var sections = new SettingsSection[]
            {
                new GeneralSection(),
                new GraphicsSection(),
                new GameplaySection(),
                new AudioSection(),
                new SkinSection(),
                new InputSection(),
                new OnlineSection(),
                new MaintenanceSection(),
                new DebugSection(),
            };
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.6f,
                },
                sectionsContainer = new SettingsSectionsContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = width,
                    Margin = new MarginPadding { Left = SIDEBAR_WIDTH },
                    ExpandableHeader = new SettingsHeader(),
                    FixedHeader = searchTextBox = new SearchTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Width = 0.95f,
                        Margin = new MarginPadding
                        {
                            Top = 20,
                            Bottom = 20
                        },
                        Exit = Hide,
                    },
                    Sections = sections,
                    Footer = new SettingsFooter()
                },
                sidebar = new Sidebar
                {
                    Width = SIDEBAR_WIDTH,
                    Children = sidebarButtons = sections.Select(section =>
                        new SidebarButton
                        {
                            Section = section,
                            Action = sectionsContainer.ScrollContainer.ScrollIntoView,
                        }
                    ).ToArray()
                }
            };

            selectedSidebarButton = sidebarButtons[0];
            selectedSidebarButton.Selected = true;

            sectionsContainer.SelectedSection.ValueChanged += section =>
            {
                selectedSidebarButton.Selected = false;
                selectedSidebarButton = sidebarButtons.Single(b => b.Section == section);
                selectedSidebarButton.Selected = true;
            };

            searchTextBox.Current.ValueChanged += newValue => sectionsContainer.SearchContainer.SearchTerm = newValue;

            sectionsContainer.Padding = new MarginPadding { Top = game?.Toolbar.DrawHeight ?? 0 };
        }

        protected override void PopIn()
        {
            base.PopIn();

            sectionsContainer.MoveToX(0, TRANSITION_LENGTH, EasingTypes.OutQuint);
            sidebar.MoveToX(0, TRANSITION_LENGTH, EasingTypes.OutQuint);
            FadeTo(1, TRANSITION_LENGTH / 2);

            searchTextBox.HoldFocus = true;
        }

        protected override void PopOut()
        {
            base.PopOut();

            sectionsContainer.MoveToX(-width, TRANSITION_LENGTH, EasingTypes.OutQuint);
            sidebar.MoveToX(-SIDEBAR_WIDTH, TRANSITION_LENGTH, EasingTypes.OutQuint);
            FadeTo(0, TRANSITION_LENGTH / 2);

            searchTextBox.HoldFocus = false;
            if (searchTextBox.HasFocus)
                InputManager.ChangeFocus(null);
        }

        public override bool AcceptsFocus => true;

        protected override bool OnClick(InputState state) => true;

        protected override void OnFocus(InputState state)
        {
            InputManager.ChangeFocus(searchTextBox);
            base.OnFocus(state);
        }

        private class SettingsSectionsContainer : SectionsContainer
        {
            public SearchContainer SearchContainer;
            private readonly Box headerBackground;

            protected override Container<Drawable> CreateScrollContentContainer()
                => SearchContainer = new SearchContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                };

            public SettingsSectionsContainer()
            {
                ScrollContainer.ScrollbarVisible = false;
                Add(headerBackground = new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.X
                });
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                // no null check because the usage of this class is strict
                headerBackground.Height = ExpandableHeader.LayoutSize.Y + FixedHeader.LayoutSize.Y;
                headerBackground.Y = ExpandableHeader.Y;
                headerBackground.Alpha = -ExpandableHeader.Y / ExpandableHeader.LayoutSize.Y * 0.5f;
            }
        }
    }
}
