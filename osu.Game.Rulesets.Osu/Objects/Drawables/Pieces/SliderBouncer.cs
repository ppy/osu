// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SliderBouncer : Container, ISliderProgress
    {
        private readonly Slider slider;
        private readonly bool isEnd;
        private readonly SpriteIcon icon;

        public SliderBouncer(Slider slider, bool isEnd)
        {
            this.slider = slider;
            this.isEnd = isEnd;

            AutoSizeAxes = Axes.Both;
            Blending = BlendingMode.Additive;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Icon = FontAwesome.fa_eercast,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(48),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            icon.Spin(1000, RotationDirection.Clockwise);
        }

        public void UpdateProgress(double progress, int repeat)
        {
            if (Time.Current < slider.StartTime)
                Alpha = 0;

            Alpha = repeat + 1 < slider.RepeatCount && repeat % 2 == (isEnd ? 0 : 1) ? 1 : 0;
        }
    }
}
