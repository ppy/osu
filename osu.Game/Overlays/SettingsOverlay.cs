// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using System;
using osu.Game.Overlays.Settings.Sections;
using osu.Framework.Input;

namespace osu.Game.Overlays
{
    public class SettingsOverlay : FocusedOverlayContainer
    {
        internal const float CONTENT_MARGINS = 10;

        public const float TRANSITION_LENGTH = 600;

        public const float SIDEBAR_WIDTH = Sidebar.DEFAULT_WIDTH;

        private const float width = 400;

        private const float sidebar_padding = 10;

        private ScrollContainer scrollContainer;
        private Sidebar sidebar;
        private SidebarButton[] sidebarButtons;
        private SettingsSection[] sections;

        private SettingsHeader header;

        private SettingsFooter footer;

        private SearchContainer searchContainer;

        private float lastKnownScroll;

        public SettingsOverlay()
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGame game)
        {
            sections = new SettingsSection[]
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
                scrollContainer = new ScrollContainer
                {
                    ScrollDraggerVisible = false,
                    RelativeSizeAxes = Axes.Y,
                    Width = width,
                    Margin = new MarginPadding { Left = SIDEBAR_WIDTH },
                    Children = new Drawable[]
                    {
                        searchContainer = new SearchContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FillDirection.Vertical,
                            Children = sections,
                        },
                        footer = new SettingsFooter(),
                        header = new SettingsHeader(() => scrollContainer.Current)
                        {
                            Exit = Hide,
                        },
                    }
                },
                sidebar = new Sidebar
                {
                    Width = SIDEBAR_WIDTH,
                    Children = sidebarButtons = sections.Select(section =>
                        new SidebarButton
                        {
                            Selected = sections[0] == section,
                            Section = section,
                            Action = () => scrollContainer.ScrollIntoView(section),
                        }
                    ).ToArray()
                }
            };

            header.SearchTextBox.Current.ValueChanged += newValue => searchContainer.SearchTerm = newValue;

            scrollContainer.Padding = new MarginPadding { Top = game?.Toolbar.DrawHeight ?? 0 };
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            //we need to update these manually because we can't put the SettingsHeader inside the SearchContainer (due to its anchoring).
            searchContainer.Y = header.DrawHeight;
            footer.Y = searchContainer.Y + searchContainer.DrawHeight;
        }

        protected override void Update()
        {
            base.Update();

            float currentScroll = scrollContainer.Current;
            if (currentScroll != lastKnownScroll)
            {
                lastKnownScroll = currentScroll;

                SettingsSection bestCandidate = null;
                float bestDistance = float.MaxValue;

                foreach (SettingsSection section in sections)
                {
                    float distance = Math.Abs(scrollContainer.GetChildPosInContent(section) - currentScroll);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCandidate = section;
                    }
                }

                var previous = sidebarButtons.SingleOrDefault(sb => sb.Selected);
                var next = sidebarButtons.SingleOrDefault(sb => sb.Section == bestCandidate);
                if (previous != null) previous.Selected = false;
                if (next != null) next.Selected = true;
            }
        }

        protected override void PopIn()
        {
            base.PopIn();

            scrollContainer.MoveToX(0, TRANSITION_LENGTH, EasingTypes.OutQuint);
            sidebar.MoveToX(0, TRANSITION_LENGTH, EasingTypes.OutQuint);
            FadeTo(1, TRANSITION_LENGTH / 2);

            header.SearchTextBox.HoldFocus = true;
        }

        protected override void PopOut()
        {
            base.PopOut();

            scrollContainer.MoveToX(-width, TRANSITION_LENGTH, EasingTypes.OutQuint);
            sidebar.MoveToX(-SIDEBAR_WIDTH, TRANSITION_LENGTH, EasingTypes.OutQuint);
            FadeTo(0, TRANSITION_LENGTH / 2);

            header.SearchTextBox.HoldFocus = false;
            header.SearchTextBox.TriggerFocusLost();
        }

        protected override bool OnFocus(InputState state)
        {
            header.SearchTextBox.TriggerFocus(state);
            return false;
        }
    }
}
