//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Game.Overlays.Options;
using osu.Game.Overlays.Options.Audio;
using osu.Game.Overlays.Options.Gameplay;
using osu.Game.Overlays.Options.General;
using osu.Game.Overlays.Options.Graphics;
using osu.Game.Overlays.Options.Input;
using osu.Game.Overlays.Options.Online;

namespace osu.Game.Overlays
{
    public class OptionsOverlay : OverlayContainer
    {
        internal const float CONTENT_MARGINS = 10;

        private const float width = 400;
        private const float sidebar_width = 60;

        private ScrollContainer scrollContainer;
        private OptionsSidebar sidebar;

        public OptionsOverlay()
        {
            var sections = new OptionsSection[]
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
                    ScrollbarOverlapsContent = false,
                    ScrollDraggerAnchor = Anchor.TopLeft,
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
                                    Colour = new Color4(235, 117, 139, 255),
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
                    Children = sections.Select(section =>
                        new OptionsSidebar.SidebarButton
                        {
                            Icon = section.Icon,
                            Action = () => scrollContainer.ScrollIntoView(section)
                        }
                    )
                }
            };
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
    }
}
