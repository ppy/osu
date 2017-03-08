// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;

namespace osu.Game.Modes.Taiko.Objects
{
    internal class TaikoConverter : HitObjectConverter<TaikoHitObject>
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

                    if (o is HitCircle)
                    {
                        h = new TaikoHitObject
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
                        ControlPoint controlPoint = beatmap.TimingPointAt(slider.StartTime, out overridePoint);
                        double origBeatLength = beatmap.BeatLengthAt(slider.StartTime);
                        float origVelocityAdjustment = overridePoint?.FloatVelocityAdjustment ?? 1;

                        double scoringDistance = 100 * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier;

                        double newSv;
                        if (origBeatLength > 0)
                            newSv = scoringDistance * 1000 / origBeatLength / origVelocityAdjustment;
                        else
                            newSv = scoringDistance;

                        double l = slider.Length * slider.RepeatCount * TaikoHitObject.SLIDER_FUDGE_FACTOR;
                        double v = newSv * TaikoHitObject.SLIDER_FUDGE_FACTOR;
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
                            Length = l,
                        };
                    }
                    else if (o is Spinner)
                    {
                        Spinner spinner = o as Spinner;

                        h = new Bash
                        {
                            StartTime = o.StartTime,
                            Sample = o.Sample,
                            Length = spinner.Length
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
