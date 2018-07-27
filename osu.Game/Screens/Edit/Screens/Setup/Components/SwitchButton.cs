// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.States;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SwitchButton : CircularContainer, IHasCurrentValue<bool>, IHasAccentColour
    {
        private readonly Box fill;
        private readonly Container switchContainer;

        private const float border_thickness = 4.5f;
        private const float switch_padding = 1.25f;
        private const float size_x = 45;
        private const float size_y = 20;

        public Bindable<bool> Current { get; } = new Bindable<bool>();

        private Color4 enabledColour;
        public Color4 EnabledColour
        {
            get => enabledColour;
            set
            {
                if (Current.Value)
                    AccentColour = value;
                enabledColour = value;
            }
        }

        private Color4 disabledColour;
        public Color4 DisabledColour
        {
            get => disabledColour;
            set
            {
                if (!Current.Value)
                    AccentColour = value;
                disabledColour = value;
            }
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                BorderColour = value;
            }
        }

        public SwitchButton()
        {
            Size = new Vector2(size_x, size_y);
            BorderColour = Color4.White;
            BorderThickness = border_thickness;
            Masking = true;

            Container switchCircle;

            Children = new Drawable[]
            {
                fill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                    Alpha = 0
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(border_thickness + switch_padding),
                    Child = switchContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = switchCircle = new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Masking = true,
                            Child = new Box { RelativeSizeAxes = Axes.Both }
                        }
                    }
                }
            };

            Current.ValueChanged += newValue =>
            {
                if (newValue)
                    switchCircle.MoveToX(switchContainer.DrawWidth - switchCircle.DrawWidth, 200, Easing.OutQuint);
                else
                    switchCircle.MoveToX(0, 200, Easing.OutQuint);
                this.FadeAccent((newValue ? enabledColour : disabledColour).Lighten(IsHovered ? 0.3f : 0), 500, Easing.OutQuint);
                fill.FadeTo(newValue ? 1 : 0, 500, Easing.OutQuint);
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            EnabledColour = colours.BlueDark;
            DisabledColour = colours.Gray3;
            switchContainer.Colour = enabledColour;
            fill.Colour = disabledColour;
        }

        protected override bool OnClick(InputState state)
        {
            Current.Value = !Current.Value;
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            this.FadeAccent((Current.Value ? enabledColour : disabledColour).Lighten(0.3f), 500, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            this.FadeAccent(Current.Value ? enabledColour : disabledColour, 500, Easing.OutQuint);
            base.OnHoverLost(state);
        }
    }
}
