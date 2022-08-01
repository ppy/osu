// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Play.HUD
{
    public class SongProgressInfo : Container
    {
        private OsuSpriteText timeCurrent;
        private OsuSpriteText timeLeft;
        private OsuSpriteText progress;

        private double startTime;
        private double endTime;

        private int? previousPercent;
        private int? previousSecond;

        private double songLength => endTime - startTime;

        private const int margin = 10;

        public double StartTime
        {
            set => startTime = value;
        }

        public double EndTime
        {
            set => endTime = value;
        }

        private GameplayClock gameplayClock;

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, GameplayClock clock)
        {
            if (clock != null)
                gameplayClock = clock;

            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        timeCurrent = new OsuSpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Colour = colours.BlueLighter,
                            Font = OsuFont.Numeric,
                        }
                    }
                },
                new Container
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        progress = new OsuSpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Colour = colours.BlueLighter,
                            Font = OsuFont.Numeric,
                        }
                    }
                },
                new Container
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        timeLeft = new OsuSpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Colour = colours.BlueLighter,
                            Font = OsuFont.Numeric,
                            Margin = new MarginPadding
                            {
                                Right = margin,
                            },
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            keepTextSpritesUpright();
        }

        protected override void Update()
        {
            base.Update();

            double time = gameplayClock?.CurrentTime ?? Time.Current;

            double songCurrentTime = time - startTime;
            int currentPercent = Math.Max(0, Math.Min(100, (int)(songCurrentTime / songLength * 100)));
            int currentSecond = (int)Math.Floor(songCurrentTime / 1000.0);

            if (currentPercent != previousPercent)
            {
                progress.Text = currentPercent.ToString() + @"%";
                previousPercent = currentPercent;
            }

            if (currentSecond != previousSecond && songCurrentTime < songLength)
            {
                timeCurrent.Text = formatTime(TimeSpan.FromSeconds(currentSecond));
                timeLeft.Text = formatTime(TimeSpan.FromMilliseconds(endTime - time));

                previousSecond = currentSecond;
            }
        }

        private string formatTime(TimeSpan timeSpan) => $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{Math.Floor(timeSpan.Duration().TotalMinutes)}:{timeSpan.Duration().Seconds:D2}";

        private void keepTextSpritesUpright()
        {
            timeCurrent.OnUpdate += (timeCurrent) => { Extensions.DrawableExtensions.KeepUpright(timeCurrent); };
            progress.OnUpdate += (timeCurrent) => { Extensions.DrawableExtensions.KeepUpright(timeCurrent); };
            timeLeft.OnUpdate += (timeCurrent) => { Extensions.DrawableExtensions.KeepUpright(timeCurrent); };
        }

    }
}
