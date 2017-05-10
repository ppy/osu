// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Play
{
    public class SongProgressInfo : Container
    {
        private OsuSpriteText timeCurrent;
        private OsuSpriteText timeLeft;
        private OsuSpriteText progress;

        private double startTime;
        private double endTime;

        private int previousPercent;
        private int previousSecond;
        private double previousTimespan;

        private const int margin = 10;

        public IClock AudioClock;

        public double StartTime { set { startTime = value; } }
        public double EndTime { set { endTime = value; } }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                timeCurrent = new OsuSpriteText
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Colour = colours.BlueLighter,
                    Font = @"Venera",
                    Margin = new MarginPadding
                    {
                        Left = margin,
                    },
                },
                progress = new OsuSpriteText
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Colour = colours.BlueLighter,
                    Font = @"Venera",
                },
                timeLeft = new OsuSpriteText
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    Colour = colours.BlueLighter,
                    Font = @"Venera",
                    Margin = new MarginPadding
                    {
                        Right = margin,
                    },
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            double songCurrentTime = AudioClock.CurrentTime - startTime;

            int currentSecond = TimeSpan.FromMilliseconds(songCurrentTime).Seconds;

            if (currentSecond != previousSecond || (previousTimespan < 0 && songCurrentTime > 0))
            {
                previousTimespan = songCurrentTime;
                previousSecond = currentSecond;

                timeCurrent.Text = ((songCurrentTime < 0) ? @"-" : @"") + TimeSpan.FromMilliseconds(songCurrentTime).ToString(@"m\:ss");
                timeLeft.Text = @"-" + TimeSpan.FromMilliseconds(endTime - AudioClock.CurrentTime).ToString(@"m\:ss");
            }

            int currentPercent = (int)(songCurrentTime / (endTime - startTime) * 100);

            if (currentPercent != previousPercent)
            {
                previousPercent = currentPercent;

                progress.Text = ((currentPercent <= 0) ? @"0" : currentPercent.ToString()) + @"%";
            }
        }
    }
}
