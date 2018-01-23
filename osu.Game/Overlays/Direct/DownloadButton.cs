// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public class DownloadButton : OsuClickableContainer
    {
        private readonly Circle circle;
        private readonly DownloadIcon iconDown;
        public bool Downloading;
        private readonly DirectPanel panel;
        private float animationStep;

        private class DownloadIcon : CircularContainer
        {
            private readonly SpriteIcon icon1;
            private readonly SpriteIcon icon2;
            public bool Animating;
            public DownloadIcon()
            {
                Children = new Drawable[]
                {
                    icon1 = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(24),
                        Colour = Color4.Gray,
                        Icon = FontAwesome.fa_angle_double_down,
                        Y = -2,
                    },
                    icon2 = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(24),
                        Colour = Color4.Gray,
                        Icon = FontAwesome.fa_angle_double_down,
                        Y = -34,
                    },
                };
            }

            protected override void Update()
            {
                base.Update();
                if (Animating && icon1.Transforms.Count == 0)
                {
                    icon1.MoveToY(30, 1000).Then().MoveToY(-2);
                    icon2.MoveToY(-2, 1000).Then().MoveToY(-34);
                }
            }
        }

        public DownloadButton(DirectPanel panel)
        {
            this.panel = panel;
            Children = new Drawable[]
            {
                circle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(32),
                    Masking = true,
                    BorderColour = Color4.Gray,
                    BorderThickness = 4f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            AlwaysPresent = true,
                            Alpha = 0,
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                },
                iconDown = new DownloadIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(32),
                    Masking = true,
                }
            };
        }

        protected override void Update()
        {
            base.Update();
            iconDown.Animating = panel.Downloading;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            circle.ScaleTo(0.9f, 1000, Easing.Out);
            iconDown.ScaleTo(0.9f, 1000, Easing.Out);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            circle.ScaleTo(1f, 500, Easing.OutElastic);
            iconDown.ScaleTo(1f, 500, Easing.OutElastic);
            return base.OnMouseUp(state, args);
        }

        protected override bool OnHover(InputState state)
        {
            circle.ScaleTo(1.1f, 500, Easing.OutElastic);
            iconDown.ScaleTo(1.1f, 500, Easing.OutElastic);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            circle.ScaleTo(1f, 500, Easing.OutElastic);
            iconDown.ScaleTo(1f, 500, Easing.OutElastic);
        }
    }
}
