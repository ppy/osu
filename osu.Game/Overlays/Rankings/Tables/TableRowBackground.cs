// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace osu.Game.Overlays.Rankings.Tables
{
    public partial class TableRowBackground : CompositeDrawable
    {
        private const int fade_duration = 100;

        private readonly Box background;

        private Color4 idleColour;
        private Color4 hoverColour;

        public TableRowBackground()
        {
            RelativeSizeAxes = Axes.X;

            CornerRadius = 4;
            Masking = true;
            MaskingSmoothness = 0.5f;

            InternalChild = background = new Box
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = idleColour = colourProvider.Background4;
            hoverColour = colourProvider.Background3;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(hoverColour, fade_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(idleColour, fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
