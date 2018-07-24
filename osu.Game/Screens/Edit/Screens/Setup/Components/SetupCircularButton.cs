// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SetupCircularButton : Container, IHasAccentColour
    {
        private readonly Box fill;
        private readonly OsuSpriteText label;

        public const float DEFAULT_LABEL_TEXT_SIZE = 14;
        public const float SIZE_X = 125;
        public const float SIZE_Y = 30;

        public event Action ButtonClicked;

        private bool disabled;
        public bool Disabled
        {
            get => disabled;
            set
            {
                disabled = value;
                fadeColour();
            }
        }

        private Color4 defaultColour;
        public Color4 DefaultColour
        {
            get => defaultColour;
            set
            {
                defaultColour = value;
                fadeColour();
            }
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                fill.Colour = value;
            }
        }

        private string labelText;
        public string LabelText
        {
            get => labelText;
            set
            {
                labelText = value;
                label.Text = value;
            }
        }

        public SetupCircularButton()
        {
            Size = new Vector2(SIZE_X, SIZE_Y);
            CornerRadius = 15;
            Masking = true;

            Children = new Drawable[]
            {
                fill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                },
                label = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.White,
                    TextSize = DEFAULT_LABEL_TEXT_SIZE,
                    Text = LabelText,
                    Font = @"Exo2.0-Bold",
                }
            };
        }

        protected override void LoadComplete()
        {
            FadeEdgeEffectTo(0);
        }

        protected override bool OnClick(InputState state)
        {
            // Effect to indicate the button has been clicked
            if (!disabled)
                ButtonClicked?.Invoke();
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            fadeColour();
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            fadeColour();
            base.OnHoverLost(state);
        }

        private void fadeColour()
        {
            if (!disabled)
            {
                this.FadeAccent(defaultColour.Lighten(IsHovered ? 0.3f : 0), 500, Easing.OutQuint);
                this.FadeTo(1, 500, Easing.OutQuint);
            }
            else
                this.FadeTo(0.3f, 500, Easing.OutQuint);
        }
    }
}
