// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Beatmaps;
using System;
using osu.Game.Beatmaps.Timing;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects
{
    class TaikoConverter : HitObjectConverter<TaikoHitObject>
    {
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
                        h = new HitCircle()
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            NewCombo = o.NewCombo,
                        };
                    }

                    Osu.Objects.Slider slider = o as Osu.Objects.Slider;

                    if (slider != null)
                    {
                        // Don't ask... Old osu! had a random multiplier here, that we now have to multiply everywhere
                        double fudgeFactor = 1.4;

                        // We compute slider velocity ourselves since we use double VelocityAdjustment here, whereas
                        // the old osu! used float. This creates a veeeeeeeeeeery tiny (on the order of 2.4092825810839713E-05) offset
                        // to slider velocity, that results in triggering the below conditional in some incorrect cases. This doesn't
                        // seem fixable by Precision.AlmostBigger(), because the error is extremely small (and is also dependent on float precision).
                        ControlPoint overridePoint;
                        ControlPoint controlPoint = beatmap.TimingPointAt(slider.StartTime, out overridePoint);
                        double origBeatLength = beatmap.BeatLengthAt(slider.StartTime, false);
                        float origVelocityAdjustment = overridePoint?.FloatVelocityAdjustment ?? 1;

                        double scoringDistance = 100 * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier;

                        double newSv;
                        if (origBeatLength > 0)
                            newSv = scoringDistance * 1000 / origBeatLength / origVelocityAdjustment;
                        else
                            newSv = scoringDistance;

                        double l = slider.Length * fudgeFactor * slider.RepeatCount;
                        double v = newSv * fudgeFactor;
                        double bl = beatmap.BeatLengthAt(slider.StartTime, false);

                        double skipPeriod = Math.Min(bl / beatmap.BeatmapInfo.BaseDifficulty.SliderTickRate, slider.Duration / slider.RepeatCount);

                        if (skipPeriod > 0 && l / v * 1000 < 2 * bl)
                        {
                            for (double j = slider.StartTime; j <= slider.EndTime + skipPeriod / 8; j += skipPeriod)
                            {
                                // Todo: This should generate circles with different sounds for when
                                // beatmap object has multiple sound additions
                                h = new HitCircle()
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

                        h = new DrumRoll()
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            NewCombo = o.NewCombo,
                            Length = (o as Osu.Objects.Slider).Length,
                            RepeatCount = (o as Osu.Objects.Slider).RepeatCount
                        };
                    }

                    if (o is Osu.Objects.Spinner)
                    {
                        h = new Bash()
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            Length = (o as Osu.Objects.Spinner).Length
                        };
                    }
                }

                h.SetDefaultsFromBeatmap(beatmap);
                output.Add(h);
            }

            return output;
        }
    }
}
