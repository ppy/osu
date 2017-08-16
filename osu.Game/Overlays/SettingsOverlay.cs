// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Overlays
{
    public abstract class SettingsOverlay : OsuFocusedOverlayContainer
    {
        internal const float CONTENT_MARGINS = 10;

        public const float TRANSITION_LENGTH = 600;

        public const float SIDEBAR_WIDTH = Sidebar.DEFAULT_WIDTH;

        private const float width = 400;

        private const float sidebar_padding = 10;

        private Sidebar sidebar;
        private SidebarButton selectedSidebarButton;

        private SettingsSectionsContainer sectionsContainer;

        private SearchTextBox searchTextBox;

        private Func<float> getToolbarHeight;

        private readonly bool showSidebar;

        protected SettingsOverlay(bool showSidebar)
        {
            this.showSidebar = showSidebar;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
        }

        protected virtual IEnumerable<SettingsSection> CreateSections() => null;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGame game)
        {
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
                    ExpandableHeader = CreateHeader(),
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
                    Footer = CreateFooter()
                }
            };

            if (showSidebar)
            {
                Add(sidebar = new Sidebar { Width = SIDEBAR_WIDTH });

                sectionsContainer.SelectedSection.ValueChanged += section =>
                {
                    selectedSidebarButton.Selected = false;
                    selectedSidebarButton = sidebar.Children.Single(b => b.Section == section);
                    selectedSidebarButton.Selected = true;
                };
            }

            searchTextBox.Current.ValueChanged += newValue => sectionsContainer.SearchContainer.SearchTerm = newValue;

            getToolbarHeight = () => game?.ToolbarOffset ?? 0;

            CreateSections()?.ForEach(AddSection);
        }

        protected void AddSection(SettingsSection section)
        {
            sectionsContainer.Add(section);

            if (sidebar != null)
            {
                var button = new SidebarButton
                {
                    Section = section,
                    Action = s =>
                    {
                        sectionsContainer.ScrollTo(s);
                        sidebar.State = ExpandedState.Contracted;
                    },
                };

                sidebar.Add(button);

                if (selectedSidebarButton == null)
                {
                    selectedSidebarButton = sidebar.Children.First();
                    selectedSidebarButton.Selected = true;
                }
            }
        }

        protected virtual Drawable CreateHeader() => new Container();

        protected virtual Drawable CreateFooter() => new Container();

        protected override void PopIn()
        {
            base.PopIn();

            sectionsContainer.MoveToX(0, TRANSITION_LENGTH, Easing.OutQuint);
            sidebar?.MoveToX(0, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(1, TRANSITION_LENGTH / 2);

            searchTextBox.HoldFocus = true;
        }

        protected override void PopOut()
        {
            base.PopOut();

            sectionsContainer.MoveToX(-width, TRANSITION_LENGTH, Easing.OutQuint);
            sidebar?.MoveToX(-SIDEBAR_WIDTH, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(0, TRANSITION_LENGTH / 2);

            searchTextBox.HoldFocus = false;
            if (searchTextBox.HasFocus)
                GetContainingInputManager().ChangeFocus(null);
        }

        public override bool AcceptsFocus => true;

        protected override bool OnClick(InputState state) => true;

        protected override void OnFocus(InputState state)
        {
            GetContainingInputManager().ChangeFocus(searchTextBox);
            base.OnFocus(state);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            sectionsContainer.Margin = new MarginPadding { Left = sidebar?.DrawWidth ?? 0 };
            sectionsContainer.Padding = new MarginPadding { Top = getToolbarHeight() };
        }

        private class SettingsSectionsContainer : SectionsContainer<SettingsSection>
        {
            public SearchContainer<SettingsSection> SearchContainer;

            protected override FlowContainer<SettingsSection> CreateScrollContentContainer()
                => SearchContainer = new SearchContainer<SettingsSection>
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                };

            public SettingsSectionsContainer()
            {
                HeaderBackground = new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both
                };
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                // no null check because the usage of this class is strict
                HeaderBackground.Alpha = -ExpandableHeader.Y / ExpandableHeader.LayoutSize.Y * 0.5f;
            }
        }
    }
}
