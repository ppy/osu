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
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class OsuColourButton : CircularContainer, IHasCurrentValue<Color4>
    {
        private bool isColourPickerOpen;
        public bool IsColourPickerOpen
        {
            get => isColourPickerOpen;
            private set
            {
                if (isColourPickerOpen == value)
                    return;

                isColourPickerOpen = value;

                if (value)
                    ShowColourPicker();
                else
                    HideColourPicker();
            }
        }

        private readonly OsuSpriteText colourLabel;
        private readonly OsuColourPicker colourPicker;
        private readonly Box fill;

        public const float COLLAPSED_SIZE = 75;
        public const float EXPANDED_SIZE = 160;
        public const float SIZE_Y = 75;
        public const float COLOUR_LABEL_TEXT_SIZE = 11;

        public OsuColourButton(bool expanded = false)
        {
            Size = new Vector2(expanded ? EXPANDED_SIZE : COLLAPSED_SIZE, SIZE_Y);

            Children = new Drawable[]
            {
                colourPicker = new OsuColourPicker
                {
                    Origin = Anchor.TopLeft,
                    X = Size.X - SIZE_Y / 2,
                    Size = new Vector2(0, SIZE_Y),
                },
                new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Child = fill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        AlwaysPresent = true,
                    },
                },
                colourLabel = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "#ffffff",
                    Colour = Color4.Black,
                    TextSize = COLOUR_LABEL_TEXT_SIZE
                },
            };

            colourPicker.ColourChanged += newColour => Current.Value = newColour;

            Current.ValueChanged += newValue =>
            {
                fill.FadeColour(newValue, 200, Easing.OutQuint);
                colourLabel.Text = toHexRGBString(newValue);
                colourLabel.Colour = Color4.ToHsv(newValue).Z >= 0.5f ? Color4.Black : Color4.White;
                // Do something to change the colour to black/white depending on the brightness of the currently used colour
            };
        }

        protected override bool OnClick(InputState state)
        {
            IsColourPickerOpen = !IsColourPickerOpen;
            
            return base.OnClick(state);
        }

        public void ShowColourPicker() => colourPicker.Expand();
        public void HideColourPicker() => colourPicker.Collapse();

        public Bindable<Color4> Current { get; } = new Bindable<Color4>();

        private string toHexRGBString(Color4 colour) => $"#{((byte)(colour.R * 255)).ToString("X2").ToLower()}{((byte)(colour.G * 255)).ToString("X2").ToLower()}{((byte)(colour.B * 255)).ToString("X2").ToLower()}";
    }
}
