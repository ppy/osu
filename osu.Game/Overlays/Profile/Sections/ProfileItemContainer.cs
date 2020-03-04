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

        private readonly Box background;
        private readonly Container content;

        private Color4 idleColour;

        protected Color4 IdleColour
        {
            get => idleColour;
            set
            {
                idleColour = value;
                if (!IsHovered)
                    background.Colour = value;
            }
        }

        private Color4 hoverColour;

        protected Color4 HoverColour
        {
            get => hoverColour;
            set
            {
                hoverColour = value;
                if (IsHovered)
                    background.Colour = value;
            }
        }

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
            IdleColour = colourProvider.Background3;
            HoverColour = colourProvider.Background2;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(HoverColour, hover_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            background.FadeColour(IdleColour, hover_duration, Easing.OutQuint);
        }
    }
}
