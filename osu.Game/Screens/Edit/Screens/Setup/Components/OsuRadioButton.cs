// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class OsuRadioButton : CircularContainer, IHasCurrentValue<bool>, IHasAccentColour
    {
        private readonly Container innerSwitch;

        public const float BORDER_THICKNESS = 5;
        public const float SIZE_X = 30;

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

        public OsuRadioButton()
        {
            Size = new Vector2(SIZE_X, 12);
            
            BorderThickness = BORDER_THICKNESS;

            Masking = true;

            Children = new Drawable[]
            {
                innerSwitch = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = new Vector2(BORDER_THICKNESS + 2),
                    Size = new Vector2(8),
                    CornerRadius = 4,
                }
            };

            Current.ValueChanged += newValue =>
            {
                if (newValue)
                    innerSwitch.MoveToX(SIZE_X - BORDER_THICKNESS - 2, 200, Easing.OutQuint);
                else
                    innerSwitch.MoveToX(BORDER_THICKNESS + 2, 200, Easing.OutQuint);
                BorderColour = newValue ? enabledColour : disabledColour;
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            innerSwitch.Colour = colours.BlueLight;

            AccentColour = colours.BlueLight;
            GlowingAccentColour = colours.BlueLighter;
            GlowColour = colours.BlueDarker;
            EnabledColour = colours.BlueLighter;
            DisabledColour = colours.Gray4;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = GlowColour,
                Type = EdgeEffectType.Glow,
                Radius = 10,
                Roundness = 8,
            };
        }

        protected override void LoadComplete()
        {
            FadeEdgeEffectTo(0);
        }

        protected override bool OnHover(InputState state)
        {
            Glowing = true;
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            Glowing = false;
            base.OnHoverLost(state);
        }
        
        private bool glowing;
        public bool Glowing
        {
            get { return glowing; }
            set
            {
                glowing = value;

                if (value)
                {
                    this.FadeColour(GlowingAccentColour, 500, Easing.OutQuint);
                    FadeEdgeEffectTo(1, 500, Easing.OutQuint);
                }
                else
                {
                    FadeEdgeEffectTo(0, 500);
                    this.FadeColour(AccentColour, 500);
                }
            }
        }

        public Bindable<bool> Current { get; } = new Bindable<bool>();

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;
                if (!Glowing)
                    Colour = value;
            }
        }

        private Color4 glowingAccentColour;
        public Color4 GlowingAccentColour
        {
            get { return glowingAccentColour; }
            set
            {
                glowingAccentColour = value;
                if (Glowing)
                    Colour = value;
            }
        }

        private Color4 glowColour;
        public Color4 GlowColour
        {
            get { return glowColour; }
            set
            {
                glowColour = value;

                var effect = EdgeEffect;
                effect.Colour = value;
                EdgeEffect = effect;
            }
        }
    }
}
