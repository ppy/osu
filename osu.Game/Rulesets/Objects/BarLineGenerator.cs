// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Objects
{
    public class BarLineGenerator<TBarLine>
        where TBarLine : class, IBarLine, new()
    {
        /// <summary>
        /// The generated bar lines.
        /// </summary>
        public readonly List<TBarLine> BarLines = new List<TBarLine>();

        /// <summary>
        /// Constructs and generates bar lines for provided beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to generate bar lines for.</param>
        public BarLineGenerator(IBeatmap beatmap)
        {
            if (beatmap.HitObjects.Count == 0)
                return;

            double firstHitTime = beatmap.HitObjects.First().StartTime;
            double lastHitTime = 1 + beatmap.GetLastObjectTime();

            var timingPoints = beatmap.ControlPointInfo.TimingPoints;

            if (timingPoints.Count == 0)
                return;

            for (int i = 0; i < timingPoints.Count; i++)
            {
                TimingControlPoint currentTimingPoint = timingPoints[i];
                int currentBeat = 0;

                // Don't generate barlines before the hit object or t=0 (whichever is earliest). Some beatmaps use very unrealistic values here (although none are ranked).
                // I'm not sure we ever want barlines to appear before the first hitobject, but let's keep some degree of compatibility for now.
                // Of note, this will still differ from stable if the first timing control point is t<0 and is not near the first hitobject.
                double generationStartTime = Math.Min(0, firstHitTime);

                // Stop on the next timing point, or if there is no next timing point stop slightly past the last object
                double endTime = i < timingPoints.Count - 1 ? timingPoints[i + 1].Time : lastHitTime + currentTimingPoint.BeatLength * currentTimingPoint.TimeSignature.Numerator;

                double barLength = currentTimingPoint.BeatLength * currentTimingPoint.TimeSignature.Numerator;

                double startTime;

                if (currentTimingPoint.Time > generationStartTime)
                {
                    startTime = currentTimingPoint.Time;
                }
                else
                {
                    // If the timing point starts before the minimum allowable time for bar lines,
                    // we still need to compute a start time for generation that is actually properly aligned with the timing point.
                    int barCount = (int)Math.Ceiling((generationStartTime - currentTimingPoint.Time) / barLength);

                    startTime = currentTimingPoint.Time + barCount * barLength;
                }

                if (currentTimingPoint.OmitFirstBarLine)
                {
                    startTime += barLength;
                }

                for (double t = startTime; Precision.AlmostBigger(endTime, t); t += barLength, currentBeat++)
                {
                    double roundedTime = Math.Round(t, MidpointRounding.AwayFromZero);

                    // in the case of some bar lengths, rounding errors can cause t to be slightly less than
                    // the expected whole number value due to floating point inaccuracies.
                    // if this is the case, apply rounding.
                    if (Precision.AlmostEquals(t, roundedTime))
                    {
                        t = roundedTime;
                    }

                    BarLines.Add(new TBarLine
                    {
                        StartTime = t,
                        Major = currentBeat % currentTimingPoint.TimeSignature.Numerator == 0
                    });
                }
            }
        }
    }
}
