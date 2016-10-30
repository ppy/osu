//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.TimeDisplay
{
    public class TimeDisplay : Container
    {
        private SpriteText timeText;

        public string Format;
        public float TextSize;
        public Vector2 TextPosition;

        public TimeDisplay()
        {
            Children = new Drawable[]
            {
                timeText = new SpriteText()
                {
                    Direction = FlowDirection.HorizontalOnly,
                }
            };
        }

        protected override void Update()
        {
            timeText.Text = DateTime.Now.ToString(format);
            timeText.TextSize = textSize;
            timeText.Position = textPos;
        }

    }
}
