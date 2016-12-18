//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SliderBouncer : Container, ISliderProgress
    {
        private readonly Slider slider;
        private readonly bool isEnd;
        private TextAwesome icon;

        public SliderBouncer(Slider slider, bool isEnd)
        {
            this.slider = slider;
            this.isEnd = isEnd;

            AutoSizeAxes = Axes.Both;
            BlendingMode = BlendingMode.Additive;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                icon = new TextAwesome
                {
                    Icon = FontAwesome.fa_eercast,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = 48,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            icon.RotateTo(360, 1000);
            icon.Loop();
        }

        public void UpdateProgress(double progress, int repeat)
        {
            if (Time.Current < slider.StartTime)
                Alpha = 0;

            Alpha = repeat + 1 < slider.RepeatCount && repeat % 2 == (isEnd ? 0 : 1) ? 1 : 0;
        }
    }
}
