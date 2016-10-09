//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Skeleton for a numeric counter with a simple roll-up animation.
    /// </summary>
    /// <typeparam name="T">Type of the actual counter.</typeparam>
    public abstract class NumericRollingCounter<T> : RollingCounter<T>
    {
        protected SpriteText countSpriteText;

        protected float textSize = 20.0f;
        public float TextSize
        {
            get { return textSize; }
            set
            {
                textSize = value;
                updateTextSize();
            }
        }

        public override void Load()
        {

            base.Load();
            Children = new Drawable[]
            {
                countSpriteText = new SpriteText
                {
                    Text = formatCount(Count),
                    TextSize = this.TextSize,
                    Anchor = this.Anchor,
                    Origin = this.Origin,
                },
            };
        }

        protected override void transformVisibleCount(T currentValue, T newValue)
        {
            if (countSpriteText != null)
            {
                countSpriteText.Text = formatCount(newValue);
            }
        }

        protected virtual void updateTextSize()
        {
            if (countSpriteText != null)
                countSpriteText.TextSize = TextSize;
        }
    }
}
