// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing.RowAttributes
{
    public partial class AttributeProgressBar : ProgressBar
    {
        private readonly ControlPoint controlPoint;

        public AttributeProgressBar(ControlPoint controlPoint)
            : base(false)
        {
            this.controlPoint = controlPoint;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider overlayColours)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            Masking = true;

            RelativeSizeAxes = Axes.None;

            Size = new Vector2(80, 8);
            CornerRadius = Height / 2;

            BackgroundColour = overlayColours.Background6;
            FillColour = controlPoint.GetRepresentingColour(colours);
        }
    }
}
