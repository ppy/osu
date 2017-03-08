// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Beatmaps;
using System;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Modes.Taiko.Objects
{
    internal class TaikoConverter : HitObjectConverter<TaikoHitObject>
    {
        /// <summary>
        /// To be honest, I don't know why this is needed. Old osu! scaled the
        /// slider multiplier by this factor, seemingly randomly, but now we unfortunately
        /// have to replicate that here anywhere slider length/slider multipliers are used :(
        /// </summary>
        private const double slider_fudge_factor = 1.4;

        public override List<TaikoHitObject> Convert(Beatmap beatmap)
        {
            List<TaikoHitObject> output = new List<TaikoHitObject>();

            foreach (HitObject i in beatmap.HitObjects)
            {
                TaikoHitObject h = i as TaikoHitObject;

                if (h == null)
                {
                    OsuHitObject o = i as OsuHitObject;

                    if (o == null)
                        throw new HitObjectConvertException(@"Taiko", i);

                    if (o is Osu.Objects.HitCircle)
                    {
                        h = new TaikoHitObject
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            NewCombo = o.NewCombo,
                        };
                    }
                    else if (o is Osu.Objects.Slider)
                    {
                        Slider slider = o as Osu.Objects.Slider;

                        // We compute slider velocity ourselves since we use double VelocityAdjustment here, whereas
                        // the old osu! used float. This creates a veeeeeeeeeeery tiny (on the order of 2.4092825810839713E-05) offset
                        // to slider velocity, that results in triggering the below conditional in some incorrect cases. This doesn't
                        // seem fixable by Precision.AlmostBigger(), because the error is extremely small (and is also dependent on float precision).
                        ControlPoint overridePoint;
                        ControlPoint controlPoint = beatmap.TimingPointAt(slider.StartTime, out overridePoint);
                        double origBeatLength = beatmap.BeatLengthAt(slider.StartTime);
                        float origVelocityAdjustment = overridePoint?.FloatVelocityAdjustment ?? 1;

                        double scoringDistance = 100 * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier;

                        double newSv;
                        if (origBeatLength > 0)
                            newSv = scoringDistance * 1000 / origBeatLength / origVelocityAdjustment;
                        else
                            newSv = scoringDistance;

                        double l = slider.Length * slider.RepeatCount * slider_fudge_factor;
                        double v = newSv * slider_fudge_factor;
                        double bl = beatmap.BeatLengthAt(slider.StartTime);

                        double skipPeriod = Math.Min(bl / beatmap.BeatmapInfo.BaseDifficulty.SliderTickRate, slider.Duration / slider.RepeatCount);

                        if (skipPeriod > 0 && l / v * 1000 < 2 * bl)
                        {
                            for (double j = slider.StartTime; j <= slider.EndTime + skipPeriod / 8; j += skipPeriod)
                            {
                                // Todo: This should generate circles with different sounds for when
                                // beatmap object has multiple sound additions
                                h = new TaikoHitObject
                                {
                                    StartTime = j,
                                    Sample = slider.Sample,
                                    NewCombo = false
                                };

                                h.SetDefaultsFromBeatmap(beatmap);
                                output.Add(h);
                            }

                            continue;
                        }

                        h = new DrumRoll
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            NewCombo = o.NewCombo,
                            // Don't add the fudge factor just yet
                            Length = slider.Length * slider.RepeatCount,
                        };
                    }
                    else if (o is Osu.Objects.Spinner)
                    {
                        Spinner spinner = o as Osu.Objects.Spinner;

                        h = new Bash
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            Length = spinner.Length
                        };
                    }
                }

                h.SetDefaultsFromBeatmap(beatmap);

                // Post-process fudge factor
                h.PreEmpt /= slider_fudge_factor;
                DrumRoll d = h as DrumRoll;
                if (d != null)
                {
                    d.Velocity *= slider_fudge_factor;
                    d.Length *= slider_fudge_factor;
                }

                output.Add(h);
            }

            return output;
        }

        public List<BarLine> ConvertBarLines(Beatmap beatmap)
        {
            List<BarLine> output = new List<BarLine>();

            double lastHitTime = beatmap.HitObjects[beatmap.HitObjects.Count - 1].EndTime + 1;

            List<ControlPoint> timingPoints = beatmap.ControlPoints?.FindAll(cp => cp.TimingChange);

            if (timingPoints == null || timingPoints.Count == 0)
                return new List<BarLine>();

            int currentIndex = 0;

            while (currentIndex < timingPoints.Count && timingPoints[currentIndex].BeatLength == 0)
                currentIndex++;

            double time = timingPoints[currentIndex].Time;
            double measureLength = timingPoints[currentIndex].BeatLength * (int)timingPoints[currentIndex].TimeSignature;

            // Find the bar line time closest to 0
            time -= measureLength * (int)(time / measureLength);

            // Always start barlines from a positive time
            while (time < 0)
                time += measureLength;

            double lastBeatLength = timingPoints[currentIndex].BeatLength;
            int currentBeat = 0;
            while (time <= lastHitTime)
            {
                ControlPoint current = timingPoints[currentIndex];

                if (time > current.Time || !current.OmitFirstBarLine)
                {
                    BarLine barLine = new BarLine
                    {
                        StartTime = time,
                        IsMajor = currentBeat % (int)current.TimeSignature == 0
                    };

                    barLine.SetDefaultsFromBeatmap(beatmap);

                    // Add fudge factor
                    barLine.PreEmpt /= slider_fudge_factor;

                    output.Add(barLine);

                    currentBeat++;
                }

                double bl = current.BeatLength;

                if (bl < 800)
                    bl *= (int)current.TimeSignature;

                time += bl;

                if (currentIndex + 1 < timingPoints.Count && time >= timingPoints[currentIndex + 1].Time)
                {
                    currentIndex++;
                    time = timingPoints[currentIndex].Time;

                    currentBeat = 0;
                }
            }

            return output;
        }
    }
}
