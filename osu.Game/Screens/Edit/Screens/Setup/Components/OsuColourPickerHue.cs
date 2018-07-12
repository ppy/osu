// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class OsuColourPickerHue : Container, IHasCurrentValue<Color4>
    {
        public const float SIZE_X = 180;
        public const float SIZE_Y = 20;
        public const float COLOUR_LABEL_TEXT_SIZE = 18;

        public event Action<Color4> HueChanged;

        public float Hue
        {
            get => Color4.ToHsv(Current.Value).X;
            set
            {
                Current.Value = Color4.FromHsv(new Vector4(value, 1, 1, 1));
                TriggerHueChanged(Current.Value);
            }
        }

        public OsuColourPickerHue()
        {
            Size = new Vector2(SIZE_X, SIZE_Y);
            CornerRadius = 10;
            Masking = true;

            loadPicker();

            Current.Value = Color4.Red;

            Current.ValueChanged += newValue =>
            {
                TriggerHueChanged(newValue);
            };
        }

        private void loadPicker()
        {
            for (float x = 0; x < SIZE_X; x++)
            {
                // Color4 treats Vector4 for HSV values as follows:
                // X = H
                // Y = S
                // Z = V
                // W = Alpha
                // All values range in [0, 1], except for hue which excludes 1, and are stored as floats
                Vector4 v = new Vector4(x / SIZE_X, 1, 1, 1);

                AddInternal(new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Colour = Color4.FromHsv(v),
                    X = x
                });
            }
        }

        private Color4 calculateColour(float x) => Color4.FromHsv(new Vector4(x / SIZE_X, 1, 1, 1));

        public void TriggerHueChanged(Color4 newValue)
        {
            HueChanged?.Invoke(newValue);
        }

        private float selectedColourXPosition = 0;

        private void setMouseStateValue(InputState state) => Current.Value = calculateColour(selectedColourXPosition = MathHelper.Clamp(state.Mouse.Position.X - Position.X, 0, SIZE_X - 1));

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
