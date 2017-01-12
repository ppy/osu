//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using OpenTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Modes.Objects
{
    /// <summary>
    /// A hitobject describes a point in a beatmap 
    /// </summary>
    public abstract class HitObject
    {
        public double StartTime;
        public virtual double EndTime => StartTime;

        public bool NewCombo { get; set; }

        public Color4 Colour = new Color4(17, 136, 170, 255);

        public double Duration => EndTime - StartTime;

        public HitSampleInfo Sample;

        public virtual void SetDefaultsFromBeatmap(Beatmap beatmap) { }
    }
}
