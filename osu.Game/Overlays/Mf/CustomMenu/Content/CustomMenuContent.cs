// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuContent : FillFlowContainer
    {
        public const float HORIZONTAL_PADDING = 30;
        private const float transition_time = 400;

        public MfMenuContent()
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_PADDING };
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Width = 0.6f,
                AutoSizeDuration = transition_time,
                AutoSizeEasing = Easing.OutQuint,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.9f,
                    },
                }
            };
        }
    }
}
