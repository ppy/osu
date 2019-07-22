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

        protected SettingsSectionsContainer SectionsContainer;
        private SettingsSection selectedSection;

        private SearchTextBox searchTextBox;

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
                AddInternal(new SidebarScrollContainer
                {
                    Child = Sidebar = new Sidebar()
                });
            }

            searchTextBox.Current.ValueChanged += term => SectionsContainer.SearchContainer.SearchTerm = term.NewValue;

            CreateSections()?.ForEach(AddSection);

            if (showSidebar)
            {
                selectedSection = SectionsContainer.Children.FirstOrDefault();

                if (selectedSection != null)
                    Sidebar.Current.Value = selectedSection;

                SectionsContainer.SelectedSection.ValueChanged += section =>
                {
                    selectedSection = section.NewValue;
                    Sidebar.Current.Value = selectedSection;
                };

                Sidebar.Current.ValueChanged += section =>
                {
                    if (selectedSection == section.NewValue)
                        return;

                    SectionsContainer.ScrollTo(section.NewValue);
                };
            }
        }

        protected void AddSection(SettingsSection section)
        {
            SectionsContainer.Add(section);
            Sidebar?.AddItem(section);
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

        private class SidebarScrollContainer : OsuScrollContainer
        {
            public SidebarScrollContainer()
            {
                RelativeSizeAxes = Axes.Y;
                AutoSizeAxes = Axes.X;

                Content.RelativeSizeAxes = Axes.None;
                Content.AutoSizeAxes = Axes.Both;
                Content.Anchor = Anchor.CentreLeft;
                Content.Origin = Anchor.CentreLeft;

                AddInternal(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Depth = 1,
                });

                ScrollbarVisible = false;
            }
        }
    }
}
