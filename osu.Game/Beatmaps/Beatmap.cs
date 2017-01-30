//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
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
        public double BPMMaximum => 60000 / ControlPoints.Where(c => c.BeatLength != 0).OrderBy(c => c.BeatLength).First().BeatLength;
        public double BPMMinimum => 60000 / ControlPoints.Where(c => c.BeatLength != 0).OrderByDescending(c => c.BeatLength).First().BeatLength;
        public double BPMMode => BPMAt(ControlPoints.Where(c => c.BeatLength != 0).GroupBy(c => c.BeatLength).OrderByDescending(grp => grp.Count()).First().First().Time);

        public double BPMAt(double time)
        {
            return 60000 / BeatLengthAt(time);
        }

        public double BeatLengthAt(double time, bool applyMultipliers = false)
        {
            int point = 0;
            int samplePoint = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
                if (ControlPoints[i].Time <= time)
                {
                    if (ControlPoints[i].TimingChange)
                        point = i;
                    else
                        samplePoint = i;
                }

            double mult = 1;

            if (applyMultipliers && samplePoint > point)
                mult = ControlPoints[samplePoint].VelocityAdjustment;

            return ControlPoints[point].BeatLength * mult;
        }
    }
}
