//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.Beatmaps.Samples;
using osu.Game.GameModes.Play;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Objects
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

        public static HitObject Parse(PlayMode mode, string val)
        {
            //TODO: move to modular HitObjectParser system rather than static parsing. (https://github.com/ppy/osu/pull/60/files#r83135780)
            switch (mode)
            {
                case PlayMode.Osu:
                    return OsuBaseHit.Parse(val);
                default:
                    return null;
            }
        }
    }
}
