using System;
using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;

namespace osu.Mods.Rulesets.Core.HitObjects
{
    public abstract class SymcolHitObject : HitObject
    {
        public SampleInfo GetAdjustedSample(SampleInfo info, SampleControlPoint point = null)
        {
            List<SampleInfo> list = new List<SampleInfo>();

            SampleControlPoint control = SampleControlPoint;

            if (point != null)
                control = point;

            info.Bank = info.Bank ?? control.SampleBank;
            info.Volume = info.Volume > 0 ? info.Volume : control.SampleVolume;
            //info.Suffix = Int32.Parse(info.Suffix ?? "0") != 0 ? info.Suffix : control.SampleSuffix.ToString();

            return info;
        }
    }
}
