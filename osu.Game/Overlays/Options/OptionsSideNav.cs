using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class OptionsSideNav : Container
    {
        private FlowContainer content;
        protected override Container Content => content;

        public OptionsSideNav()
        {
            RelativeSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                content = new FlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Direction = FlowDirection.VerticalOnly
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

        public class SidebarButton : Container
        {
            private TextAwesome drawableIcon;
            private Box backgroundBox;
            public Action Action;
            
            public FontAwesome Icon
            {
                get { return drawableIcon.Icon; }
                set { drawableIcon.Icon = value; }
            }
            
            public SidebarButton()
            {
                Size = new Vector2(60);
                Children = new Drawable[]
                {
                    backgroundBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        BlendingMode = BlendingMode.Additive,
                        Colour = new Color4(60, 60, 60, 255),
                        Alpha = 0,
                    },
                    drawableIcon = new TextAwesome
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }
            
            protected override bool OnMouseDown(InputState state, MouseDownEventArgs e)
            {
                Action?.Invoke();
                backgroundBox.FlashColour(Color4.White, 400);
                return true;
            }
            
            protected override bool OnHover(InputState state)
            {
                backgroundBox.FadeTo(0.4f, 200);
                return true;
            }
    
            protected override void OnHoverLost(InputState state)
            {
                backgroundBox.FadeTo(0, 200);
            }
        }
    }
}