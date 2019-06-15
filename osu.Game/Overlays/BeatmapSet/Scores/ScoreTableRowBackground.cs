// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTableRowBackground : CompositeDrawable
    {
        private const int fade_duration = 100;

        private readonly Box hoveredBackground;
        private readonly Box background;

        public ScoreTableRowBackground(int index)
        {
            RelativeSizeAxes = Axes.X;
            Height = 25;

            CornerRadius = 3;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                hoveredBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
            };

            if (index % 2 != 0)
                background.Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoveredBackground.Colour = colours.Gray4;
            background.Colour = colours.Gray3;
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoveredBackground.FadeIn(fade_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoveredBackground.FadeOut(fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
