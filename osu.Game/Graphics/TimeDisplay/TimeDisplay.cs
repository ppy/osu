//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;

namespace osu.Game.Graphics.TimeDisplay
{
    class TimeDisplayContainer : LargeContainer
    {
        private TimeDisplay timeDisplay;

        public TimeDisplayContainer()
        {
            Add(timeDisplay = new TimeDisplay());
        }

        class TimeDisplay : Drawable
        {
            private SpriteText timeText;
            private DateTime dateTime;

            public TimeDisplay()
            {
                dateTime = new DateTime();
                Add(timeText = new SpriteText());

                OnUpdate += TimeDisplay_OnUpdate;
            }

            private void TimeDisplay_OnUpdate()
            {
                timeText.Text = DateTime.Now.ToString("h:mm:ss tt");
            }

        }
    }
}
