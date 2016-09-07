//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.TimeDisplay
{
    public class TimeDisplay : Drawable
    {
        private SpriteText timeText;
        private bool _24Hour;

        public override void Load()
        {
            base.Load();

            Add(timeText = new SpriteText()
            {
                Direction = FlowDirection.HorizontalOnly
            });
        }

        protected override void Update()
        {
            if (_24Hour)
                timeText.Text = DateTime.Now.ToString("HH:mm:ss");
            else
                timeText.Text = DateTime.Now.ToString("h:mm:ss tt");
        }

        public bool millitaryTime
        {
            set { _24Hour = value; }
        }
    }
}
