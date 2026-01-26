// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
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

        private readonly HoverSounds sounds;

        public FormControlBackground()
        {
            RelativeSizeAxes = Axes.Both;

            Masking = true;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            BorderThickness = 2.5f;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both,
                },
                sounds = new HoverSounds(),
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
            sounds.Enabled.Value = !styleDisabled;

            ColourInfo colour = colourProvider.Background4.Darken(0.1f);
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

            this.TransformTo(nameof(BorderColour), border ? borderColour : colour, 250, Easing.OutQuint);

            box.FadeColour(colour, 250, Easing.OutQuint);
        }
    }
}
