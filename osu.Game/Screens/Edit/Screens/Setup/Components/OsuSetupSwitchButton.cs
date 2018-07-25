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
    public class OsuSetupSwitchButton : CircularContainer, IHasCurrentValue<bool>, IHasAccentColour
    {
        private readonly Box fill;
        private readonly Container switchContainer;

        public const float BORDER_THICKNESS = 4.5f;
        public const float SWITCH_PADDING = 1.25f;
        public const float SIZE_X = 45;
        public const float SIZE_Y = 20;

        private Color4 enabledColour;
        public Color4 EnabledColour
        {
            get => enabledColour;
            set
            {
                if (Current.Value)
                    BorderColour = value;
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
                    BorderColour = value;
                disabledColour = value;
            }
        }

        public OsuSetupSwitchButton()
        {
            Box innerSwitch;

            Size = new Vector2(SIZE_X, SIZE_Y);

            BorderColour = Color4.White;
            BorderThickness = BORDER_THICKNESS;

            Masking = true;

            Children = new Drawable[]
            {
                fill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                    Alpha = 0
                },
                switchContainer = new Container
                {
                    CornerRadius = SIZE_Y / 2 - BORDER_THICKNESS - SWITCH_PADDING,
                    Masking = true,
                    Size = new Vector2(SIZE_Y - 2 * BORDER_THICKNESS - 2 * SWITCH_PADDING),
                    Position = new Vector2(BORDER_THICKNESS + SWITCH_PADDING),
                    Child = innerSwitch = new Box
                    {
                        Size = new Vector2(SIZE_Y - 2 * BORDER_THICKNESS - 2 * SWITCH_PADDING),
                    }
                }
            };

            Current.ValueChanged += newValue =>
            {
                if (newValue)
                    switchContainer.MoveToX(SIZE_X - BORDER_THICKNESS - SWITCH_PADDING - innerSwitch.Size.X, 200, Easing.OutQuint);
                else
                    switchContainer.MoveToX(BORDER_THICKNESS + SWITCH_PADDING, 200, Easing.OutQuint);
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

        protected override void LoadComplete()
        {
            FadeEdgeEffectTo(0);
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

        public Bindable<bool> Current { get; } = new Bindable<bool>();

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
    }
}
