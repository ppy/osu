//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
            ControlPoint point = ControlPointAt(time);
            double mult = 1;

            if (!point.TimingChange)
            {
                if (applyMultipliers)
                    mult = point.VelocityAdjustment;
                point = ControlPointAt(time, true);
            }

            return point.BeatLength * mult;
        }

        public ControlPoint ControlPointAt(double time) =>
            ControlPoints.FindLast(controlPoint => controlPoint.Time <= time);

        public ControlPoint ControlPointAt(double time, bool timingChange) =>
            ControlPoints.FindLast(controlPoint => controlPoint.Time <= time && controlPoint.TimingChange == timingChange);
    }
}
