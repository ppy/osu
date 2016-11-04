using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options
{
    public class OptionsSideNav : Container
    {
        public Action GeneralAction;
        public Action GraphicsAction;
        public Action GameplayAction;
        public Action AudioAction;
        public Action SkinAction;
        public Action InputAction;
        public Action EditorAction;
        public Action OnlineAction;
        public Action MaintenanceAction;
    
        public OptionsSideNav()
        {
            RelativeSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new FlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Direction = FlowDirection.VerticalOnly,
                    Children = new[]
                    {
                        new SidebarButton
                        {
                            Icon = Graphics.FontAwesome.gear,
                            Action = () => GeneralAction(),
                        },
                        new SidebarButton
                        {
                            Icon = Graphics.FontAwesome.laptop,
                            Action = () => GraphicsAction(),
                        },
                        new SidebarButton
                        {
                            Icon = Graphics.FontAwesome.circle_o,
                            Action = () => GameplayAction(),
                        },
                        new SidebarButton
                        {
                            Icon = Graphics.FontAwesome.headphones,
                            Action = () => AudioAction(),
                        },
                        new SidebarButton
                        {
                            Icon = Graphics.FontAwesome.fa_paint_brush,
                            Action = () => SkinAction(),
                        },
                        new SidebarButton
                        {
                            Icon = Graphics.FontAwesome.keyboard_o,
                            Action = () => InputAction(),
                        },
                        new SidebarButton
                        {
                            Icon = Graphics.FontAwesome.pencil,
                            Action = () => EditorAction(),
                        },
                        new SidebarButton
                        {
                            Icon = Graphics.FontAwesome.globe,
                            Action = () => {
                                OnlineAction();
                            }
                        },
                        new SidebarButton
                        {
                            Icon = Graphics.FontAwesome.wrench,
                            Action = () => MaintenanceAction(),
                        }
                    }
                },
                new Box
                {
                    Colour = new Color4(30, 30, 30, 255),
                    RelativeSizeAxes = Axes.Y,
                    Width = 2,
                    Origin = Anchor.TopRight,
                    Anchor = Anchor.TopRight,
                }
            };
        }

        private class SidebarButton : Container
        {
            private ToolbarButton button;
            
            public Action Action
            {
                get { return button.Action; }
                set { button.Action = value; }
            }
            
            public Graphics.FontAwesome Icon
            {
                get { return button.Icon; }
                set { button.Icon = value; }
            }
            
            public SidebarButton()
            {
                Size = new Vector2(60);
                Children = new[] { button = new ToolbarButton() };
            }
        }
    }
}