// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineBreak : CompositeDrawable
    {
        public BreakPeriod Break { get; }

        public TimelineBreak(BreakPeriod b)
        {
            Break = b;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.TopLeft;
            X = (float)Break.StartTime;
            Width = (float)Break.Duration;
            CornerRadius = 10;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.GreyCarmineLight,
                    Alpha = 0.4f,
                },
                new Circle
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = 10,
                    CornerRadius = 5,
                    Colour = colours.GreyCarmineLighter,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Text = "Break",
                    Margin = new MarginPadding
                    {
                        Left = 16,
                        Top = 3,
                    },
                    Colour = colours.GreyCarmineLighter,
                },
                new Circle
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = 10,
                    CornerRadius = 5,
                    Colour = colours.GreyCarmineLighter,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Text = "Break",
                    Margin = new MarginPadding
                    {
                        Right = 16,
                        Top = 3,
                    },
                    Colour = colours.GreyCarmineLighter,
                },
            };
        }
    }
}
