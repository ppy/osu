using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class OptionsSidebar : Container
    {
        private FlowContainer content;
        internal const int default_width = 60, expanded_width = 200;
        protected override Container<Drawable> Content => content;

        public OptionsSidebar()
        {
            RelativeSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                new SidebarScrollContainer
                {
                    Children = new []
                    {
                        content = new FlowContainer
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FlowDirection.VerticalOnly
                        }
                    }
                },
            };
        }

        private ScheduledDelegate expandEvent;
        
        protected override bool OnHover(InputState state)
        {
            expandEvent = Scheduler.AddDelayed(() =>
            {
                expandEvent = null;
                ResizeTo(new Vector2(expanded_width, Height), 150, EasingTypes.OutQuad);
            }, 750);
            return true;
        }
        
        protected override void OnHoverLost(InputState state)
        {
            expandEvent?.Cancel();
            ResizeTo(new Vector2(default_width, Height), 150, EasingTypes.OutQuad);
            base.OnHoverLost(state);
        }

        private class SidebarScrollContainer : ScrollContainer
        {
            public SidebarScrollContainer()
            {
                Content.Anchor = Anchor.CentreLeft;
                Content.Origin = Anchor.CentreLeft;
            }
        }

        public class SidebarButton : Container
        {
            private TextAwesome drawableIcon;
            private SpriteText headerText;
            private Box backgroundBox;
            public Action Action;

            public FontAwesome Icon
            {
                get { return drawableIcon.Icon; }
                set { drawableIcon.Icon = value; }
            }
            
            public string Header
            {
                get { return headerText.Text; }
                set { headerText.Text = value; }
            }

            public SidebarButton()
            {
                Height = default_width;
                RelativeSizeAxes = Axes.X;
                Children = new Drawable[]
                {
                    backgroundBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        BlendingMode = BlendingMode.Additive,
                        Colour = new Color4(60, 60, 60, 255),
                        Alpha = 0,
                    },
                    new Container
                    {
                        Width = default_width,
                        RelativeSizeAxes = Axes.Y,
                        Children = new[]
                        {
                            drawableIcon = new TextAwesome
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                        }
                    },
                    headerText = new SpriteText
                    {
                        Position = new Vector2(default_width + 10, 0),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    }
                };
            }

            protected override bool OnClick(InputState state)
            {
                Action?.Invoke();
                backgroundBox.FlashColour(Color4.White, 400);
                return true;
            }

            protected override bool OnHover(InputState state)
            {
                backgroundBox.FadeTo(0.4f, 200);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                backgroundBox.FadeTo(0, 200);
                base.OnHoverLost(state);
            }
        }
    }
}