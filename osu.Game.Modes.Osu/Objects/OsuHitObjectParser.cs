// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Globalization;
using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects
{
    public class OsuHitObjectParser : HitObjectParser
    {
        public override HitObject Parse(string text)
        {
            string[] split = text.Split(',');
            var type = (HitObjectType)int.Parse(split[3]);
            bool combo = type.HasFlag(HitObjectType.NewCombo);
            type &= (HitObjectType)0xF;
            type &= ~HitObjectType.NewCombo;
            OsuHitObject result;
            switch (type)
            {
                case HitObjectType.Circle:
                    result = new HitCircle
                    {
                        Position = new Vector2(int.Parse(split[0]), int.Parse(split[1]))
                    };
                    break;
                case HitObjectType.Slider:
                    CurveTypes curveType = CurveTypes.Catmull;
                    int repeatCount;
                    double length = 0;
                    List<Vector2> points = new List<Vector2>();

                    points.Add(new Vector2(int.Parse(split[0]), int.Parse(split[1])));

                    string[] pointsplit = split[5].Split('|');
                    for (int i = 0; i < pointsplit.Length; i++)
                    {
                        if (pointsplit[i].Length == 1)
                        {
                            switch (pointsplit[i])
                            {
                                case @"C":
                                    curveType = CurveTypes.Catmull;
                                    break;
                                case @"B":
                                    curveType = CurveTypes.Bezier;
                                    break;
                                case @"L":
                                    curveType = CurveTypes.Linear;
                                    break;
                                case @"P":
                                    curveType = CurveTypes.PerfectCurve;
                                    break;
                            }
                            continue;
                        }

                        string[] temp = pointsplit[i].Split(':');
                        Vector2 v = new Vector2(
                            (int)Convert.ToDouble(temp[0], CultureInfo.InvariantCulture),
                            (int)Convert.ToDouble(temp[1], CultureInfo.InvariantCulture)
                        );
                        points.Add(v);
                    }

                    repeatCount = Convert.ToInt32(split[6], CultureInfo.InvariantCulture);

                    if (repeatCount > 9000)
                    {
                        throw new ArgumentOutOfRangeException("wacky man");
                    }

                    if (split.Length > 7)
                        length = Convert.ToDouble(split[7], CultureInfo.InvariantCulture);

                    result = new Slider
                    {
                        ControlPoints = points,
                        Length = length,
                        CurveType = curveType,
                        RepeatCount = repeatCount,
                        Position = new Vector2(int.Parse(split[0]), int.Parse(split[1]))
                    };
                    break;
                case HitObjectType.Spinner:
                    result = new Spinner
                    {
                        Length = Convert.ToDouble(split[5], CultureInfo.InvariantCulture) - Convert.ToDouble(split[2], CultureInfo.InvariantCulture),
                        Position = new Vector2(512, 384) / 2,
                    };
                    break;
                default:
                    throw new InvalidOperationException($@"Unknown hit object type {type}");
            }
            result.StartTime = Convert.ToDouble(split[2], CultureInfo.InvariantCulture);
            result.Sample = new HitSampleInfo
            {
                Type = (SampleType)int.Parse(split[4]),
                Set = SampleSet.Soft,
            };
            result.NewCombo = combo;
            // TODO: "addition" field
            return result;
        }
    }
}
