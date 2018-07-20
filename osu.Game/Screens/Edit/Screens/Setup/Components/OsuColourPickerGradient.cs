// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class OsuColourPickerGradient : Container, IHasCurrentValue<Color4>
    {
        private bool isColourChangedFromGradient;
        private bool isColourChangedFromActiveColour;

        private readonly Container pickerContainer;

        public const float SIZE_X = 180;
        public const float SIZE_Y = 180;
        public const float COLOUR_LABEL_TEXT_SIZE = 18;

        public event Action<Color4> SelectedColourChanged;

        public void TriggerSelectedColourChanged(Color4 newValue)
        {
            SelectedColourChanged?.Invoke(newValue);
        }

        private Color4 activeColour;
        public Color4 ActiveColour
        {
            get => activeColour;
            set
            {
                if (value == activeColour)
                    return;

                activeColour = value;
                pickerContainer.Clear();
                loadPicker();
                isColourChangedFromActiveColour = true;
                Current.Value = calculateColour(selectedColourPosition);
                isColourChangedFromActiveColour = false;
            }
        }

        public float ActiveHue => Color4.ToHsv(activeColour).X;

        public OsuColourPickerGradient()
        {
            CircularContainer colourPickerButton;

            Size = new Vector2(SIZE_X, SIZE_Y);

            Children = new Drawable[]
            {
                pickerContainer = new Container
                {
                    Size = new Vector2(SIZE_X, SIZE_Y),
                    CornerRadius = 10,
                    Masking = true,
                },
                colourPickerButton = new CircularContainer
                {
                    AlwaysPresent = true,
                    RelativeSizeAxes = Axes.None,
                    RelativePositionAxes = Axes.None,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.TopLeft,
                    Size = new Vector2(15),
                    //Position = new Vector2(50),
                    BorderThickness = 2,
                    BorderColour = Color4.White,
                }
            };

            loadPicker();

            Current.ValueChanged += newValue =>
            {
                if (!isColourChangedFromActiveColour)
                {
                    if (!isColourChangedFromGradient)
                    {
                        var hsv = Color4.ToHsv(newValue);
                        selectedColourPosition.X = hsv.Y * SIZE_X;
                        selectedColourPosition.Y = (1 - hsv.Z) * SIZE_Y;
                    }
                    colourPickerButton.Position = selectedColourPosition;
                }
                colourPickerButton.Colour = newValue;
                TriggerSelectedColourChanged(newValue);
            };
        }

        private void loadPicker()
        {
            pickerContainer.Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
            });
            Color4 transparentBlack = new Color4(0, 0, 0, 0);
            for (float x = 0; x < SIZE_X; x++)
            {
                float white = (SIZE_X - x) / SIZE_X;
                float r = (1 - white) * activeColour.R + white;
                float g = (1 - white) * activeColour.G + white;
                float b = (1 - white) * activeColour.B + white;

                pickerContainer.Add(new Box
                {
                    Blending = new BlendingParameters
                    {
                        AlphaEquation = BlendingEquation.Inherit,
                        RGBEquation = BlendingEquation.Inherit,
                        Mode = BlendingMode.Additive
                    },
                    RelativeSizeAxes = Axes.Y,
                    Colour = ColourInfo.GradientVertical(new Color4(r, g, b, 1), transparentBlack),
                    X = x
                });
            }
        }

        private Color4 calculateColour(Vector2 position) => calculateColour(position.X, position.Y);

        private Color4 calculateColour(float x, float y)
        {
            float s = x / SIZE_X;
            float v = (SIZE_Y - y) / SIZE_Y;
            return Color4.FromHsv(new Vector4(ActiveHue, s, v, 1));
        }

        private Vector2 selectedColourPosition = new Vector2(0);

        private void setMouseStateValue(InputState state)
        {
            float x = MathHelper.Clamp(state.Mouse.Position.X - Position.X, 0, SIZE_X);
            float y = MathHelper.Clamp(state.Mouse.Position.Y - Position.Y, 0, SIZE_Y);
            isColourChangedFromGradient = true;
            Current.Value = calculateColour(selectedColourPosition = new Vector2(x, y));
            isColourChangedFromGradient = false;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            setMouseStateValue(state);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            setMouseStateValue(state);
            return base.OnDrag(state);
        }

        public Bindable<Color4> Current { get; } = new Bindable<Color4>();
    }
}
