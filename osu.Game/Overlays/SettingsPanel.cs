// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Overlays
{
    public abstract class SettingsPanel : OsuFocusedOverlayContainer
    {
        public const float CONTENT_MARGINS = 15;

        public const float TRANSITION_LENGTH = 600;

        private const float sidebar_width = Sidebar.DEFAULT_WIDTH;

        protected const float WIDTH = 400;

        protected Container<Drawable> ContentContainer;

        protected override Container<Drawable> Content => ContentContainer;

        protected Sidebar Sidebar;
        private SidebarButton selectedSidebarButton;

        protected SettingsSectionsContainer SectionsContainer;

        private SeekLimitedSearchTextBox searchTextBox;

        /// <summary>
        /// Provide a source for the toolbar height.
        /// </summary>
        public Func<float> GetToolbarHeight;

        private readonly bool showSidebar;

        protected Box Background;

        protected SettingsPanel(bool showSidebar)
        {
            this.showSidebar = showSidebar;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
        }

        protected virtual IEnumerable<SettingsSection> CreateSections() => null;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = ContentContainer = new Container
            {
                Width = WIDTH,
                RelativeSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    Background = new Box
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Scale = new Vector2(2, 1), // over-extend to the left for transitions
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.05f),
                        Alpha = 1,
                    },
                    SectionsContainer = new SettingsSectionsContainer
                    {
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
                        ExpandableHeader = CreateHeader(),
                        FixedHeader = searchTextBox = new SeekLimitedSearchTextBox
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
                        },
                        Footer = CreateFooter()
                    },
                }
            };

            if (showSidebar)
            {
                AddInternal(Sidebar = new Sidebar { Width = sidebar_width });

                SectionsContainer.SelectedSection.ValueChanged += section =>
                {
                    selectedSidebarButton.Selected = false;
                    selectedSidebarButton = Sidebar.Children.Single(b => b.Section == section.NewValue);
                    selectedSidebarButton.Selected = true;
                };
            }

            searchTextBox.Current.ValueChanged += term => SectionsContainer.SearchContainer.SearchTerm = term.NewValue;

            CreateSections()?.ForEach(AddSection);
        }

        protected void AddSection(SettingsSection section)
        {
            SectionsContainer.Add(section);

            if (Sidebar != null)
            {
                var button = new SidebarButton
                {
                    Section = section,
                    Action = () =>
                    {
                        SectionsContainer.ScrollTo(section);
                        Sidebar.State = ExpandedState.Contracted;
                    },
                };

                Sidebar.Add(button);

                if (selectedSidebarButton == null)
                {
                    selectedSidebarButton = Sidebar.Children.First();
                    selectedSidebarButton.Selected = true;
                }
            }
        }

        protected virtual Drawable CreateHeader() => new Container();

        protected virtual Drawable CreateFooter() => new Container();

        protected override void PopIn()
        {
            base.PopIn();

            ContentContainer.MoveToX(ExpandedPosition, TRANSITION_LENGTH, Easing.OutQuint);

            Sidebar?.MoveToX(0, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(1, TRANSITION_LENGTH, Easing.OutQuint);

            searchTextBox.HoldFocus = true;
        }

        protected virtual float ExpandedPosition => 0;

        protected override void PopOut()
        {
            base.PopOut();

            ContentContainer.MoveToX(-WIDTH, TRANSITION_LENGTH, Easing.OutQuint);

            Sidebar?.MoveToX(-sidebar_width, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(0, TRANSITION_LENGTH, Easing.OutQuint);

            searchTextBox.HoldFocus = false;
            if (searchTextBox.HasFocus)
                GetContainingInputManager().ChangeFocus(null);
        }

        public override bool AcceptsFocus => true;

        protected override void OnFocus(FocusEvent e)
        {
            searchTextBox.TakeFocus();
            base.OnFocus(e);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            ContentContainer.Margin = new MarginPadding { Left = Sidebar?.DrawWidth ?? 0 };
            Padding = new MarginPadding { Top = GetToolbarHeight?.Invoke() ?? 0 };
        }

        protected class SettingsSectionsContainer : SectionsContainer<SettingsSection>
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
                HeaderBackground.Alpha = -ExpandableHeader.Y / ExpandableHeader.LayoutSize.Y;
            }
        }
    }
}
