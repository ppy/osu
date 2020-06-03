// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Graphics
{
    public class DrawableDate : OsuSpriteText, IHasCustomTooltip
    {
        private DateTimeOffset date;

        public DateTimeOffset Date
        {
            get => date;
            set
            {
                if (date == value)
                    return;

                date = value.ToLocalTime();

                if (LoadState >= LoadState.Ready)
                    updateTime();
            }
        }

        public DrawableDate(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
        {
            Font = OsuFont.GetFont(weight: FontWeight.Regular, size: textSize, italics: italic);
            Date = date;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            updateTime();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.Add(updateTimeWithReschedule);
        }

        private void updateTimeWithReschedule()
        {
            updateTime();

            var diffToNow = DateTimeOffset.Now.Subtract(Date);

            double timeUntilNextUpdate = 1000;

            if (Math.Abs(diffToNow.TotalSeconds) > 120)
            {
                timeUntilNextUpdate *= 60;

                if (Math.Abs(diffToNow.TotalMinutes) > 120)
                {
                    timeUntilNextUpdate *= 60;

                    if (Math.Abs(diffToNow.TotalHours) > 48)
                        timeUntilNextUpdate *= 24;
                }
            }

            Scheduler.AddDelayed(updateTimeWithReschedule, timeUntilNextUpdate);
        }

        protected virtual string Format() => HumanizerUtils.Humanize(Date);

        private void updateTime() => Text = Format();

        public ITooltip GetCustomTooltip() => new DateTooltip();

        public object TooltipContent => Date;

        private class DateTooltip : VisibilityContainer, ITooltip
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

            public bool SetContent(object content)
            {
                if (!(content is DateTimeOffset date))
                    return false;

                dateText.Text = $"{date:d MMMM yyyy} ";
                timeText.Text = $"{date:HH:mm:ss \"UTC\"z}";
                return true;
            }

            public void Move(Vector2 pos) => Position = pos;
        }
    }
}
