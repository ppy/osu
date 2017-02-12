// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using System.Collections.Generic;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SliderTicksRenderer : Container<DrawableSliderTick>
    {
        private double startTime;
        public double StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                update();
            }
        }

        private double repeatDuration;
        public double RepeatDuration
        {
            get { return repeatDuration; }
            set
            {
                repeatDuration = value;
                update();
            }
        }

        private IEnumerable<SliderTick> ticks;
        public IEnumerable<SliderTick> Ticks
        {
            get { return ticks; }
            set
            {
                ticks = value;
                update();
            }
        }

        public bool ShouldHit
        {
            set
            {
                foreach (var tick in Children)
                    tick.ShouldHit = value;
            }
        }

        private void update()
        {
            Clear();
            if (ticks == null || repeatDuration == 0)
                return;

            foreach (var tick in ticks)
            {
                var repeatStartTime = startTime + tick.RepeatIndex * repeatDuration;
                var fadeInTime = repeatStartTime + (tick.StartTime - repeatStartTime) / 2 - (tick.RepeatIndex == 0 ? DrawableOsuHitObject.TIME_FADEIN : DrawableOsuHitObject.TIME_FADEIN / 2);
                var fadeOutTime = repeatStartTime + repeatDuration;

                Add(new DrawableSliderTick(tick)
                {
                    FadeInTime = fadeInTime,
                    FadeOutTime = fadeOutTime,
                    Position = tick.Position,
                });
            }
        }
    }
}