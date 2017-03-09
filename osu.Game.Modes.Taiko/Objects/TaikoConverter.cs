// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Taiko.Objects
{
    internal class TaikoConverter : HitObjectConverter<TaikoHitObject>
    {
        // osu-stable adds a scale factor of 1.4 to all taiko slider velocities.
        private const double scale_factor = 1.4;

        public override List<TaikoHitObject> Convert(Beatmap beatmap)
        {
            bool alreadyPostProcessed = false;

            foreach (var c in beatmap.Timing.ControlPoints)
            {
                if ((c.EffectFlags & EffectFlags.PostProcessed) > 0)
                {
                    alreadyPostProcessed = true;
                    continue;
                }

                c.VelocityAdjustment /= scale_factor;
                c.EffectFlags |= EffectFlags.PostProcessed;
            }

            var converted = new List<TaikoHitObject>();

            Action<TaikoHitObject> addConverted = h =>
            {
                if (!alreadyPostProcessed)
                {
                    DrumRoll d = h as DrumRoll;
                    if (d != null)
                        d.Length *= scale_factor;
                }

                converted.Add(h);
                h.SetDefaultsFromBeatmap(beatmap);
            };

            foreach (HitObject h in beatmap.HitObjects)
            {
                TaikoHitObject taikoObject = h as TaikoHitObject;

                if (taikoObject == null)
                {
                    foreach (var c in convert(h, beatmap))
                        addConverted(c);
                }
                else
                    addConverted(taikoObject);
            }

            return converted;
        }

        private IEnumerable<TaikoHitObject> convert(HitObject h, Beatmap beatmap)
        {
            OsuHitObject o = h as OsuHitObject;

            if (o == null)
                throw new HitObjectConvertException(@"Taiko", h);

            if (o is HitCircle)
            {
                yield return new TaikoHitObject
                {
                    StartTime = o.StartTime,
                    Sample = o.Sample,
                    NewCombo = o.NewCombo,
                };
            }
            else if (o is Slider)
            {
                Slider slider = o as Slider;

                // We compute slider velocity ourselves since we use double VelocityAdjustment here, whereas
                // the old osu! used float. This creates a veeeeeeeeeeery tiny (on the order of 2.4092825810839713E-05) offset
                // to slider velocity, that results in triggering the below conditional in some incorrect cases. This doesn't
                // seem fixable by Precision.AlmostBigger(), because the error is extremely small (and is also dependent on float precision).
                ControlPoint overridePoint;
                beatmap.Timing.TimingPointAt(slider.StartTime, out overridePoint);
                double origBeatLength = beatmap.Timing.BeatLengthAt(slider.StartTime);
                double origVelocityAdjustment = overridePoint?.VelocityAdjustment * scale_factor ?? 1;

                double scoringDistance = 100 * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier;

                double newSv;
                if (origBeatLength > 0)
                    newSv = scoringDistance * 1000 / origBeatLength / (float)origVelocityAdjustment;
                else
                    newSv = scoringDistance;

                double l = slider.Length * slider.RepeatCount * scale_factor;
                double v = newSv * scale_factor;
                double bl = beatmap.Timing.BeatLengthAt(slider.StartTime);

                double skipPeriod = Math.Min(bl / beatmap.BeatmapInfo.BaseDifficulty.SliderTickRate, slider.Duration / slider.RepeatCount);

                if (skipPeriod > 0 && l / v * 1000 < 2 * bl)
                {
                    for (double j = slider.StartTime; j <= slider.EndTime + skipPeriod / 8; j += skipPeriod)
                    {
                        // Todo: This should generate circles with different sounds for when
                        // beatmap object has multiple sound additions
                        yield return new TaikoHitObject
                        {
                            StartTime = j,
                            Sample = slider.Sample,
                            NewCombo = false
                        };
                    }
                }
                else
                {
                    yield return new DrumRoll
                    {
                        StartTime = o.StartTime,
                        Sample = o.Sample,
                        NewCombo = o.NewCombo,
                        Length = slider.Length * slider.RepeatCount,
                    };
                }
            }
            else if (o is Spinner)
            {
                Spinner spinner = o as Spinner;

                yield return new Bash
                {
                    StartTime = o.StartTime,
                    Sample = o.Sample,
                    Length = spinner.Length
                };
            }
        }

        public List<BarLine> ConvertBarLines(Beatmap beatmap)
        {
            List<BarLine> output = new List<BarLine>();

            double lastHitTime = beatmap.HitObjects[beatmap.HitObjects.Count - 1].EndTime + 1;

            List<ControlPoint> timingPoints = beatmap.Timing?.ControlPoints.FindAll(cp => cp.TimingChange);

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

            int currentBeat = 0;
            while (time <= lastHitTime)
            {
                ControlPoint current = timingPoints[currentIndex];

                if (time > current.Time || (current.EffectFlags & EffectFlags.OmitFirstBarLine) > 0)
                {
                    BarLine barLine = new BarLine
                    {
                        StartTime = time,
                        IsMajor = currentBeat % (int)current.TimeSignature == 0
                    };

                    barLine.SetDefaultsFromBeatmap(beatmap);

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
