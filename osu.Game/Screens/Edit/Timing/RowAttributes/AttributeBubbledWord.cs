// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Timing.RowAttributes
{
    public class AttributeBubbledWord : CompositeDrawable
    {
        private readonly ControlPoint controlPoint;

        private OsuSpriteText textDrawable;

        private string text;

        public string Text
        {
            get => text;
            set
            {
                if (value == text)
                    return;

                text = value;
                if (textDrawable != null)
                    textDrawable.Text = text;
            }
        }

        public AttributeBubbledWord(ControlPoint controlPoint)
        {
            this.controlPoint = controlPoint;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider overlayColours)
        {
            AutoSizeAxes = Axes.X;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            Height = 12;

            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    Colour = controlPoint.GetRepresentingColour(colours),
                    RelativeSizeAxes = Axes.Both,
                },
                textDrawable = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding(6),
                    Font = OsuFont.Default.With(weight: FontWeight.SemiBold, size: 12),
                    Text = text,
                    Colour = overlayColours.Background5,
                },
            };
        }
    }
}
