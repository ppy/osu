// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;

namespace osu.Game.Modes.Taiko.Objects
{
    public class BarLine
    {
        public double StartTime;
        public double PreEmpt;

        public void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            PreEmpt = 600 / (beatmap.SliderVelocityAt(StartTime) * TaikoHitObject.SLIDER_FUDGE_FACTOR) * 1000;
        }
    }
}
