//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    public class ToolbarButton : Container
    {
        public const float WIDTH = 60;

        public FontAwesome Icon
        {
            get { return DrawableIcon.Icon; }
            set { DrawableIcon.Icon = value; }
        }

        public string Text
        {
            get { return DrawableText.Text; }
            set
            {
                DrawableText.Text = value;
                paddingIcon.Alpha = string.IsNullOrEmpty(value) ? 0 : 1;
            }
        }

        public string TooltipMain
        {
            get { return tooltip1.Text; }
            set
            {
                tooltip1.Text = value;
            }
        }

        public string TooltipSub
        {
            get { return tooltip2.Text; }
            set
            {
                tooltip2.Text = value;
            }
        }

        public Action Action;
        protected TextAwesome DrawableIcon;
        protected SpriteText DrawableText;
        protected Box HoverBackground;
        private Drawable paddingLeft;
        private Drawable paddingRight;
        private Drawable paddingIcon;
        private FlowContainer tooltipContainer;
        private SpriteText tooltip1;
        private SpriteText tooltip2;

        public new float Padding
        {
            get { return paddingLeft.Size.X; }
            set
            {
                paddingLeft.Size = new Vector2(value, 1);
                paddingRight.Size = new Vector2(value, 1);
                tooltipContainer.Position = new Vector2(value, tooltipContainer.Position.Y);
            }
        }

        public ToolbarButton()
        {
            HoverBackground = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Additive = true,
                Colour = new Color4(60, 60, 60, 255),
                Alpha = 0,
            };

            DrawableIcon = new TextAwesome
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            };

            DrawableText = new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            };

            tooltipContainer = new FlowContainer
            {
                Direction = FlowDirection.VerticalOnly,
                Anchor = Anchor.BottomLeft,
                Position = new Vector2(0, -5),
                Alpha = 0,
                Children = new[]
                {
                    tooltip1 = new SpriteText()
                    {
                        TextSize = 22,
                    },
                    tooltip2 = new SpriteText
                    {
                        TextSize = 15
                    }
                }
            };

            paddingLeft = new Container { RelativeSizeAxes = Axes.Y };
            paddingRight = new Container { RelativeSizeAxes = Axes.Y };
            paddingIcon = new Container
            {
                Size = new Vector2(5, 0),
                Alpha = 0
            };

            Padding = 10;
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(WIDTH, 1);
        }

        public override void Load()
        {
            base.Load();

            Children = new Drawable[]
            {
                    HoverBackground,
                    new FlowContainer
                    {
                        Direction = FlowDirection.HorizontalOnly,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            paddingLeft,
                            DrawableIcon,
                            paddingIcon,
                            DrawableText,
                            paddingRight
                        },
                    },
                    tooltipContainer
            };
        }

        protected override bool OnClick(InputState state)
        {
            Action?.Invoke();
            HoverBackground.FlashColour(Color4.White, 400);
            return true;
        }

        protected override bool OnHover(InputState state)
        {
            HoverBackground.FadeTo(0.4f, 200);
            tooltipContainer.FadeIn(100);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            HoverBackground.FadeTo(0, 200);
            tooltipContainer.FadeOut(100);
        }
    }
}