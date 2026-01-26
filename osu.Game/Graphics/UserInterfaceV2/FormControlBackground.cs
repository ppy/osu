// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormControlBackground : CompositeDrawable
    {
        private bool styleDisabled;

        public bool StyleDisabled
        {
            set
            {
                styleDisabled = value;
                updateStyling();
            }
        }

        private bool styleFocused;

        public bool StyleFocused
        {
            set
            {
                styleFocused = value;
                updateStyling();
            }
        }

        private bool styleHovered;

        public bool StyleHovered
        {
            set
            {
                styleHovered = value;
                updateStyling();
            }
        }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly Box box;

        public FormControlBackground()
        {
            RelativeSizeAxes = Axes.Both;

            Masking = true;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateStyling();
            FinishTransforms(true);
        }

        public void Flash()
        {
            box.FlashColour(ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark2), 800, Easing.OutQuint);
        }

        private void updateStyling()
        {
            ColourInfo colour = colourProvider.Background5;
            ColourInfo borderColour = colourProvider.Light4;
            bool border = false;

            if (styleDisabled)
            {
                colour = colourProvider.Background4;
                borderColour = colourProvider.Dark1;
            }
            else if (styleFocused)
            {
                colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark3);
                border = true;
                borderColour = colourProvider.Highlight1;
            }
            else if (styleHovered)
            {
                colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark4);
                border = true;
            }

            BorderThickness = border ? 2 : 0;
            BorderColour = borderColour;

            box.FadeColour(colour, 500, Easing.OutQuint);
        }
    }
}
