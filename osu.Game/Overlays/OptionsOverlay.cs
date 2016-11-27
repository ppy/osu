﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Overlays.Options;
using osu.Game.Overlays.Options.Audio;
using osu.Game.Overlays.Options.Gameplay;
using osu.Game.Overlays.Options.General;
using osu.Game.Overlays.Options.Graphics;
using osu.Game.Overlays.Options.Input;
using osu.Game.Overlays.Options.Online;
using System;

namespace osu.Game.Overlays
{
    public class OptionsOverlay : OverlayContainer
    {
        internal const float CONTENT_MARGINS = 10;

        private const float width = 400;
        private const float sidebar_width = OptionsSidebar.default_width;
        private const float sidebar_padding = 10;

        private ScrollContainer scrollContainer;
        private OptionsSidebar sidebar;
        private SidebarButton[] sidebarButtons;
        private OptionsSection[] sections;
        private float lastKnownScroll;

        public OptionsOverlay()
        {
            sections = new OptionsSection[]
            {
                new GeneralSection(),
                new GraphicsSection(),
                new GameplaySection(),
                new AudioSection(),
                new SkinSection(),
                new InputSection(),
                new EditorSection(),
                new OnlineSection(),
                new MaintenanceSection(),
            };

            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

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
                    Margin = new MarginPadding { Left = sidebar_width },
                    Children = new[]
                    {
                        new FlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FlowDirection.VerticalOnly,

                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Text = "settings",
                                    TextSize = 40,
                                    Margin = new MarginPadding { Left = CONTENT_MARGINS, Top = 30 },
                                },
                                new SpriteText
                                {
                                    Colour = new Color4(255, 102, 170, 255),
                                    Text = "Change the way osu! behaves",
                                    TextSize = 18,
                                    Margin = new MarginPadding { Left = CONTENT_MARGINS, Bottom = 30 },
                                },
                                new FlowContainer
                                {
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                    Direction = FlowDirection.VerticalOnly,
                                    Children = sections,
                                }
                            }
                        }
                    }
                },
                sidebar = new OptionsSidebar
                {
                    Width = sidebar_width,
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
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGame game)
        {
            scrollContainer.Padding = new MarginPadding { Top = game?.Toolbar.DrawHeight ?? 0 };
        }

        protected override void Update()
        {
            base.Update();

            float currentScroll = scrollContainer.Current;
            if (currentScroll != lastKnownScroll)
            {
                lastKnownScroll = currentScroll;

                OptionsSection bestCandidate = null;
                float bestDistance = float.MaxValue;

                foreach (OptionsSection section in sections)
                {
                    float distance = Math.Abs(scrollContainer.GetChildYInContent(section) - currentScroll);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCandidate = section;
                    }
                }

                var previous = sidebarButtons.SingleOrDefault(sb => sb.Selected);
                var next = sidebarButtons.SingleOrDefault(sb => sb.Section == bestCandidate);
                if (next != null)
                {
                    previous.Selected = false;
                    next.Selected = true;
                }
            }
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    if (State == Visibility.Hidden) return false;
                    Hide();
                    return true;
                case Key.PageUp:
                    if (State == Visibility.Hidden) return false;
                    ScrollToPrevSection(); // Go to start of previous section
                    return true;

                case Key.PageDown:
                    if (State == Visibility.Hidden) return false;
                    ScrollToNextSection(); // Go to start of next section
                    return true;

                    if (State == Visibility.Hidden) return false;
                    ScrollToPrevSection(); // Go to start of previous section
                    return true;
            }
            return base.OnKeyDown(state, args);
        }

        protected override void PopIn()
        {
            scrollContainer.MoveToX(0, 600, EasingTypes.OutQuint);
            sidebar.MoveToX(0, 800, EasingTypes.OutQuint);
            FadeTo(1, 300);
        }

        protected override void PopOut()
        {
            scrollContainer.MoveToX(-width, 600, EasingTypes.OutQuint);
            sidebar.MoveToX(-sidebar_width, 600, EasingTypes.OutQuint);
            FadeTo(0, 300);
        }

        private int GetCurrentSection()
        {
            int currSection = int.MaxValue;
            float minDistance = float.MaxValue;

            for (int i = 0; i < sections.Length; i++)
            {
                float distance = Math.Abs(scrollContainer.GetChildYInContent(sections[i]) - scrollContainer.Current);
                if (distance < minDistance)
                {
                    currSection = i;
                    minDistance = distance;
                }
            }

            return currSection;
        }

        private void ScrollToNextSection()
        {
            int currSection = GetCurrentSection();

            if (currSection == sections.Length - 1)
                currSection = 0;
            else
                currSection++;

            scrollContainer.ScrollIntoView(sections[currSection]);
        }

        private void ScrollToPrevSection()
        {
            int currSection = GetCurrentSection();

            if (currSection == 0)
                currSection = sections.Length - 1;
            else
                currSection--;

            scrollContainer.ScrollIntoView(sections[currSection]);
        }
    }
}
