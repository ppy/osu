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
            bool token;

            for (int i=0; i<format.Length; i++)
            {
                token = false;

                if(i < format.Length - 2)
                {
                    if ((format[i] == '[') && (format[i + 2] == ']'))
                    {
                        token = true;
                        format = format.Remove(i, 1);
                        format = format.Remove(i + 1, 1);
                        i++;
                    }
                }

                if(i < format.Length - 3)
                {
                    if ((format[i] == '[') && (format[i + 3] == ']'))
                    {
                        token = true;
                        format = format.Remove(i, 1);
                        format = format.Remove(i + 2, 1);
                        i+=2;
                    }
                }
                
                if(!token)
                {
                    if (format[i] != ' ')
                        format = format.Insert(i++, @"\");
                }
            }

            return format;
        }
    }
}
