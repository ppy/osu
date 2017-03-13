// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A Beatmap containing converted HitObjects.
    /// </summary>
    public class Beatmap<T>
        where T : HitObject
    {
        public BeatmapInfo BeatmapInfo;
        public List<ControlPoint> ControlPoints;
        public List<Color4> ComboColors;

        public BeatmapMetadata Metadata => BeatmapInfo?.Metadata ?? BeatmapInfo?.BeatmapSet?.Metadata;

        /// <summary>
        /// The HitObjects this Beatmap contains.
        /// </summary>
        public List<T> HitObjects;

        /// <summary>
        /// Constructs a new beatmap.
        /// </summary>
        /// <param name="original">The original beatmap to use the parameters of.</param>
        public Beatmap(Beatmap original = null)
        {
            BeatmapInfo = original?.BeatmapInfo;
            ControlPoints = original?.ControlPoints;
            ComboColors = original?.ComboColors;
        }

        public double BPMMaximum => 60000 / (ControlPoints?.Where(c => c.BeatLength != 0).OrderBy(c => c.BeatLength).FirstOrDefault() ?? ControlPoint.Default).BeatLength;
        public double BPMMinimum => 60000 / (ControlPoints?.Where(c => c.BeatLength != 0).OrderByDescending(c => c.BeatLength).FirstOrDefault() ?? ControlPoint.Default).BeatLength;
        public double BPMMode => BPMAt(ControlPoints.Where(c => c.BeatLength != 0).GroupBy(c => c.BeatLength).OrderByDescending(grp => grp.Count()).First().First().Time);

        public double BPMAt(double time)
        {
            return 60000 / BeatLengthAt(time);
        }

        public double BeatLengthAt(double time)
        {
            ControlPoint overridePoint;
            ControlPoint timingPoint = TimingPointAt(time, out overridePoint);
            return timingPoint.BeatLength;
        }

        public ControlPoint TimingPointAt(double time, out ControlPoint overridePoint)
        {
            overridePoint = null;

            ControlPoint timingPoint = null;
            foreach (var controlPoint in ControlPoints)
            {
                // Some beatmaps have the first timingPoint (accidentally) start after the first HitObject(s).
                // This null check makes it so that the first ControlPoint that makes a timing change is used as
                // the timingPoint for those HitObject(s).
                if (controlPoint.Time <= time || timingPoint == null)
                {
                    if (controlPoint.TimingChange)
                    {
                        timingPoint = controlPoint;
                        overridePoint = null;
                    }
                    else overridePoint = controlPoint;
                }
                else break;
            }

            return timingPoint ?? ControlPoint.Default;
        }
    }

    /// <summary>
    /// A Beatmap containing un-converted HitObjects.
    /// </summary>
    public class Beatmap : Beatmap<HitObject>
    {
        /// <summary>
        /// Calculates the star difficulty for this Beatmap.
        /// </summary>
        /// <returns>The star difficulty.</returns>
        public double CalculateStarDifficulty() => Ruleset.GetRuleset(BeatmapInfo.Mode).CreateDifficultyCalculator(this).Calculate();
    }
}
