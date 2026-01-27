// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        public const float CORNER_EXPONENT = 2.5f;
        public const float BORDER_THICKNESS = 2.5f;

        private VisualStyle visualStyle;

        public VisualStyle VisualStyle
        {
            get => visualStyle;
            set
            {
                visualStyle = value;
                updateStyle();
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

            CornerExponent = CORNER_EXPONENT;
            BorderThickness = BORDER_THICKNESS;

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

            updateStyle();
            FinishTransforms(true);
        }

        public void Flash()
        {
            box.FlashColour(ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark2), 800, Easing.OutQuint);
        }

        private void updateStyle()
        {
            sounds.Enabled.Value = visualStyle != VisualStyle.Disabled;

            ColourInfo colour;
            ColourInfo borderColour;

            bool border = false;

            switch (visualStyle)
            {
                case VisualStyle.Normal:
                    colour = colourProvider.Background4.Darken(0.1f);
                    borderColour = colourProvider.Light4;
                    break;

                case VisualStyle.Disabled:
                    colour = colourProvider.Background4;
                    borderColour = colourProvider.Dark1;
                    break;

                case VisualStyle.Hovered:
                    colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark4);
                    borderColour = colourProvider.Light4;
                    border = true;
                    break;

                case VisualStyle.Focused:
                    colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark3);
                    border = true;
                    borderColour = colourProvider.Highlight1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.TransformTo(nameof(BorderColour), border ? borderColour : colour, 250, Easing.OutQuint);

            box.FadeColour(colour, 250, Easing.OutQuint);
        }
    }

    public enum VisualStyle
    {
        Normal,
        Disabled,
        Hovered,
        Focused
    }
}
