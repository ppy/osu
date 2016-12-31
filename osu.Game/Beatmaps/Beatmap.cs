//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Modes.Objects;

namespace osu.Game.Beatmaps
{
    public class Beatmap
    {
        public BeatmapInfo BeatmapInfo { get; set; }
        public BeatmapMetadata Metadata => BeatmapInfo?.Metadata ?? BeatmapInfo?.BeatmapSet?.Metadata;
        public List<HitObject> HitObjects { get; set; }
        public List<ControlPoint> ControlPoints { get; set; }
        public List<Color4> ComboColors { get; set; }

        public double BeatLengthAt(double time, bool applyMultipliers = false)
        {
            UninheritedControlPoint pointUninherited = ControlPointAt(time, false) as UninheritedControlPoint;
            if (pointUninherited == null)
                throw new ArgumentException("Cannot get BeatLength before the first UninheritedControlPoint");

            if (applyMultipliers)
            {
                InheritedControlPoint pointInherited = ControlPointAt(time, true) as InheritedControlPoint;
                if (pointInherited == null || pointUninherited.Time > pointInherited.Time)
                    return pointUninherited.BeatLength;
                return pointUninherited.BeatLength * pointInherited.VelocityMultiplier;
            }
            return pointUninherited.BeatLength;
        }

        public ControlPoint ControlPointAt(double time) =>
            ControlPoints.FindLast(point => point.Time <= time);

        public ControlPoint ControlPointAt(double time, bool inherited) =>
            ControlPoints.FindLast(point => point.Time <= time && point is InheritedControlPoint == inherited);
    }
}
