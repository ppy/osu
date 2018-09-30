// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Select
{
    public class FooterButton : OsuClickableContainer
    {
        private static readonly Vector2 shearing = new Vector2(0.15f, 0);

        public string Text
        {
            get { return spriteText?.Text; }
            set
            {
                if (spriteText != null)
                    spriteText.Text = value;
            }
        }

        private Color4 deselectedColour;
        public Color4 DeselectedColour
        {
            get { return deselectedColour; }
            set
            {
                deselectedColour = value;
                if (light.Colour != SelectedColour)
                    light.Colour = value;
            }
        }

        private Color4 selectedColour;
        public Color4 SelectedColour
        {
            get { return selectedColour; }
            set
            {
                selectedColour = value;
                box.Colour = selectedColour;
            }
        }

        private readonly SpriteText spriteText;
        private readonly Box box;
        private readonly Box light;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => box.ReceivePositionalInputAt(screenSpacePos);

        public FooterButton()
        {
            Children = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = shearing,
                    EdgeSmoothness = new Vector2(2, 0),
                    Colour = Color4.White,
                    Alpha = 0,
                },
                light = new Box
                {
                    Shear = shearing,
                    Height = 4,
                    EdgeSmoothness = new Vector2(2, 0),
                    RelativeSizeAxes = Axes.X,
                },
                spriteText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }

        public Action Hovered;
        public Action HoverLost;
        public Key? Hotkey;

        protected override bool OnHover(InputState state)
        {
            Hovered?.Invoke();
            light.ScaleTo(new Vector2(1, 2), Footer.TRANSITION_LENGTH, Easing.OutQuint);
            light.FadeColour(SelectedColour, Footer.TRANSITION_LENGTH, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            HoverLost?.Invoke();
            light.ScaleTo(new Vector2(1, 1), Footer.TRANSITION_LENGTH, Easing.OutQuint);
            light.FadeColour(DeselectedColour, Footer.TRANSITION_LENGTH, Easing.OutQuint);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            box.FadeTo(0.3f, Footer.TRANSITION_LENGTH * 2, Easing.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            box.FadeOut(Footer.TRANSITION_LENGTH, Easing.OutQuint);
            return base.OnMouseUp(state, args);
        }

        protected override bool OnClick(InputState state)
        {
            box.ClearTransforms();
            box.Alpha = 1;
            box.FadeOut(Footer.TRANSITION_LENGTH * 3, Easing.OutQuint);
            return base.OnClick(state);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat && args.Key == Hotkey)
            {
                OnClick(state);
                return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
