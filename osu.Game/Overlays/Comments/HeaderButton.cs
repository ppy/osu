// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Comments
{
    public partial class HeaderButton : Container
    {
        private const int transition_duration = 200;

        protected override Container<Drawable> Content => content;

        private readonly Box background;
        private readonly Container content;

        public HeaderButton()
        {
            AutoSizeAxes = Axes.X;
            Height = 20;
            Masking = true;
            CornerRadius = 3;
            AddRangeInternal(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                content = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Horizontal = 10 }
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background3;
        }

        protected override bool OnHover(HoverEvent e)
        {
            ShowBackground();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            HideBackground();
        }

        protected void ShowBackground() => background.FadeIn(transition_duration, Easing.OutQuint);

        protected void HideBackground() => background.FadeOut(transition_duration, Easing.OutQuint);
    }
}
