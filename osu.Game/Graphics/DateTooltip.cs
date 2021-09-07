// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics
{
    public class DateTooltip : VisibilityContainer, ITooltip<DateTimeOffset>
    {
        private readonly OsuSpriteText dateText, timeText;
        private readonly Box background;

        public DateTooltip()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding(10),
                    Children = new Drawable[]
                    {
                        dateText = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                        },
                        timeText = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.GreySeafoamDarker;
            timeText.Colour = colours.BlueLighter;
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void SetContent(DateTimeOffset date)
        {
            dateText.Text = $"{date:d MMMM yyyy} ";
            timeText.Text = $"{date:HH:mm:ss \"UTC\"z}";
        }

        public void Move(Vector2 pos) => Position = pos;
    }
}
