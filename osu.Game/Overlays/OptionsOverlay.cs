//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Graphics;

namespace osu.Game.Overlays
{
    public class OptionsOverlay : OverlayContainer
    {
        internal const float CONTENT_MARGINS = 10;

        public const float TRANSITION_LENGTH = 600;

        public const float SIDEBAR_WIDTH = OptionsSidebar.default_width;

        private const float width = 400;
        
        private const float sidebar_padding = 10;

        private ScrollContainer scrollContainer;
        private OptionsSidebar sidebar;
        private SidebarButton[] sidebarButtons;
        private OptionsSection[] sections;
        private float lastKnownScroll;

        public OptionsOverlay()
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGame game, OsuColour colours)
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
                                    Colour = colours.Pink,
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
            }
            return base.OnKeyDown(state, args);
        }

        protected override void PopIn()
        {
            scrollContainer.MoveToX(0, TRANSITION_LENGTH, EasingTypes.OutQuint);
            sidebar.MoveToX(0, TRANSITION_LENGTH, EasingTypes.OutQuint);
            FadeTo(1, TRANSITION_LENGTH / 2);
        }

        protected override void PopOut()
        {
            scrollContainer.MoveToX(-width, TRANSITION_LENGTH, EasingTypes.OutQuint);
            sidebar.MoveToX(-SIDEBAR_WIDTH, TRANSITION_LENGTH, EasingTypes.OutQuint);
            FadeTo(0, TRANSITION_LENGTH / 2);
        }
    }
}
