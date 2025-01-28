// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterfaceV2.FileSelection
{
    internal partial class BackgroundLayer : CompositeDrawable
    {
        private Box background = null!;

        private readonly float defaultAlpha;

        public BackgroundLayer(float defaultAlpha = 0f)
        {
            Depth = float.MaxValue;

            this.defaultAlpha = defaultAlpha;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider overlayColourProvider)
        {
            RelativeSizeAxes = Axes.Both;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new HoverClickSounds(),
                background = new Box
                {
                    Alpha = defaultAlpha,
                    Colour = overlayColourProvider.Background3,
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeTo(1, 200, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            background.FadeTo(defaultAlpha, 500, Easing.OutQuint);
        }
    }
}
