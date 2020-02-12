// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Sections
{
    public class ProfileItemContainer : Container
    {
        private const int hover_duration = 200;

        protected override Container<Drawable> Content => content;

        private Color4 idleColour;
        private Color4 hoverColour;

        private readonly Box background;
        private readonly Container content;

        public ProfileItemContainer()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 6;

            AddRangeInternal(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = idleColour = colourProvider.Background3;
            hoverColour = colourProvider.Background2;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(hoverColour, hover_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            background.FadeColour(idleColour, hover_duration, Easing.OutQuint);
        }
    }
}
