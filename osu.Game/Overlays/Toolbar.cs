//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Configuration;

namespace osu.Game.Overlays
{
    public class Toolbar : Container
    {
        const float height = 50;
        private FlowContainer leftFlow;
        private FlowContainer rightFlow;

        public override void Load()
        {
            base.Load();

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, height);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0.1f, 0.1f, 0.1f, 0.9f)
                },
                leftFlow = new FlowContainer
                {
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(0, 1),
                    Children = new []
                    {
                        new ToolbarButton(FontAwesome.gear),
                        new ToolbarButton(FontAwesome.home),
                        new ToolbarModeButton(FontAwesome.fa_osu_osu_o, @"osu!"),
                        new ToolbarModeButton(FontAwesome.fa_osu_taiko_o, @"taiko"),
                        new ToolbarModeButton(FontAwesome.fa_osu_fruits_o, @"catch"),
                        new ToolbarModeButton(FontAwesome.fa_osu_mania_o, @"mania"),
                    }
                },
                rightFlow = new FlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(0, 1),
                    Children = new []
                    {
                        new ToolbarButton(FontAwesome.search),
                        new ToolbarButton(FontAwesome.user, ((OsuGame)Game).Config.Get<string>(OsuConfig.Username)),
                        new ToolbarButton(FontAwesome.bars),
                    }
                }
            };
        }

        public class ToolbarModeButton : ToolbarButton
        {
            public ToolbarModeButton(FontAwesome icon, string text = null) : base(icon, text)
            {
            }

            public override void Load()
            {
                base.Load();
                Icon.TextSize = height * 0.7f;
            }
        }

        public class ToolbarButton : FlowContainer
        {
            public TextAwesome Icon;
            public SpriteText Text;
            private Box background;
            private Drawable paddingLeft;
            private Drawable paddingRight;
            private Drawable paddingIcon;

            public new float Padding
            {
                get { return paddingLeft.Size.X; }
                set
                {
                    paddingLeft.Size = new Vector2(value, 1);
                    paddingRight.Size = new Vector2(value, 1);
                }
            }

            public ToolbarButton(FontAwesome icon, string text = null)
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(20, 20, 20, 140),
                };

                this.Icon = new TextAwesome()
                {
                    Icon = icon,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                };

                this.Text = new SpriteText
                {
                    Text = text,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                };

                paddingLeft = new Container { RelativeSizeAxes = Axes.Y };
                paddingRight = new Container { RelativeSizeAxes = Axes.Y };
                paddingIcon = new Container { Size = new Vector2(string.IsNullOrEmpty(text) ? 0 : 5, 0) };

                Padding = 10;
            }

            protected override bool OnHover(InputState state)
            {
                background.FadeColour(new Color4(130, 130, 130, 160), 100);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                background.FadeColour(new Color4(20, 20, 20, 140), 100);
                base.OnHoverLost(state);
            }

            public override void Load()
            {
                base.Load();

                RelativeSizeAxes = Axes.Y;
                Direction = FlowDirection.HorizontalOnly;

                Children = new Drawable[]
                {
                    background,
                    paddingLeft,
                    Icon,
                    paddingIcon,
                    Text,
                    paddingRight,
                };
            }
        }
    }
}
