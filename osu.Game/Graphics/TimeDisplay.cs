//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.TimeDisplay
{
    public class TimeDisplay : Container
    {
        private SpriteText timeText;
        private string format;

        public override void Load()
        {
            base.Load();
            Children = new Drawable[]
            {
                timeText = new SpriteText()
                {
                    Direction = FlowDirection.HorizontalOnly
                }
            };
        }

        protected override void Update()
        {
            timeText.Text = DateTime.Now.ToString(format);
        }

        public bool MilitaryTime { get; set; }
        public string Format
        {
            set
            {
                format = ProcessFormat(value);
            }
        }

        private string ProcessFormat(string format)
        {
            format = format.Insert(0, "\"");
            format = format.Insert(format.Length - 1, "\"");

            int start = 0;
            int end = 0;

            while (start != -1 && end != -1)
            {
                 start = format.IndexOf('[', start + 1);
                 end = format.IndexOf(']', end + 1);

                if ((end - start == 2) || (end - start == 3))
                {
                    format = format.Remove(start, 1);
                    format = format.Remove(end - 1, 1);

                    format = format.Insert(start, "\"");
                    format = format.Insert(end, "\"");
                }
            }

            return format;
        }
    }
}
