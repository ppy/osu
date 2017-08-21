﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
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

        protected const float WIDTH = 400;

        private const float sidebar_padding = 10;

        protected Container<Drawable> ContentContainer;

        protected override Container<Drawable> Content => ContentContainer;

        protected Sidebar Sidebar;
        private SidebarButton selectedSidebarButton;

        protected SettingsSectionsContainer SectionsContainer;

        private SearchTextBox searchTextBox;

        /// <summary>
        /// Provide a source for the toolbar height.
        /// </summary>
        public Func<float> GetToolbarHeight;

        private readonly bool showSidebar;

        protected Box Background;

        protected SettingsOverlay(bool showSidebar)
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
                        Colour = Color4.Black,
                        Alpha = 0.6f,
                    },
                    SectionsContainer = new SettingsSectionsContainer
                    {
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
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
                    },
                }
            };

            if (showSidebar)
            {
                AddInternal(Sidebar = new Sidebar { Width = SIDEBAR_WIDTH });

                SectionsContainer.SelectedSection.ValueChanged += section =>
                {
                    selectedSidebarButton.Selected = false;
                    selectedSidebarButton = Sidebar.Children.Single(b => b.Section == section);
                    selectedSidebarButton.Selected = true;
                };
            }

            searchTextBox.Current.ValueChanged += newValue => SectionsContainer.SearchContainer.SearchTerm = newValue;

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
                    Action = s =>
                    {
                        SectionsContainer.ScrollTo(s);
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

            Sidebar?.MoveToX(-SIDEBAR_WIDTH, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(0, TRANSITION_LENGTH, Easing.OutQuint);

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

            ContentContainer.Margin = new MarginPadding { Left = Sidebar?.DrawWidth ?? 0 };
            ContentContainer.Padding = new MarginPadding { Top = GetToolbarHeight?.Invoke() ?? 0 };
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
                HeaderBackground.Alpha = -ExpandableHeader.Y / ExpandableHeader.LayoutSize.Y * 0.5f;
            }
        }
    }
}
