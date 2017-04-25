// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Options;
using System;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Options.Sections;

namespace osu.Game.Overlays
{
    public class OptionsOverlay : FocusedOverlayContainer
    {
        internal const float CONTENT_MARGINS = 10;

        public const float TRANSITION_LENGTH = 600;

        public const float SIDEBAR_WIDTH = Sidebar.DEFAULT_WIDTH;

        private const float width = 400;

        private const float sidebar_padding = 10;

        private ScrollContainer scrollContainer;
        private Sidebar sidebar;
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
                    Children = new[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FillDirection.Vertical,

                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "settings",
                                    TextSize = 40,
                                    Margin = new MarginPadding { Left = CONTENT_MARGINS, Top = Toolbar.Toolbar.TOOLTIP_HEIGHT },
                                },
                                new OsuSpriteText
                                {
                                    Colour = colours.Pink,
                                    Text = "Change the way osu! behaves",
                                    TextSize = 18,
                                    Margin = new MarginPadding { Left = CONTENT_MARGINS, Bottom = 30 },
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                    Direction = FillDirection.Vertical,
                                    Children = sections,
                                },
                                new OptionsFooter()
                            }
                        }
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
        }

        protected override void PopOut()
        {
            base.PopOut();

            scrollContainer.MoveToX(-width, TRANSITION_LENGTH, EasingTypes.OutQuint);
            sidebar.MoveToX(-SIDEBAR_WIDTH, TRANSITION_LENGTH, EasingTypes.OutQuint);
            FadeTo(0, TRANSITION_LENGTH / 2);
        }
    }
}
