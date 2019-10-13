// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Comments
{
    public class HeaderButton : Container
    {
        private const int height = 20;
        private const int corner_radius = 3;
        private const int margin = 10;
        private const int duration = 200;

        protected override Container<Drawable> Content => content;

        private readonly Box background;
        private readonly Container content;

        public HeaderButton()
        {
            AutoSizeAxes = Axes.X;
            Height = height;
            Masking = true;
            CornerRadius = corner_radius;
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
                    Margin = new MarginPadding { Horizontal = margin }
                },
                new HoverClickSounds(),
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray4;
        }

        protected override bool OnHover(HoverEvent e)
        {
            FadeInBackground();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            FadeOutBackground();
        }

        protected void FadeInBackground() => background.FadeIn(duration, Easing.OutQuint);

        protected void FadeOutBackground() => background.FadeOut(duration, Easing.OutQuint);
    }
}
