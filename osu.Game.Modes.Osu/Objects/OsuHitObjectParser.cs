//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects
{
    public class OsuHitObjectParser : HitObjectParser
    {
        public override HitObject Parse(Beatmap beatmap, string text)
        {
            string[] split = text.Split(',');
            if (split.Length < 5)
                throw new ArgumentException("OsuHitObjects must have at least 5 values");

            var type = (OsuHitObject.HitObjectType)Convert.ToInt32(split[3], NumberFormatInfo.InvariantInfo);
            bool combo = type.HasFlag(OsuHitObject.HitObjectType.NewCombo);
            int comboColourOffset = ((int)type & 0x70) >> 4;
            type &= (OsuHitObject.HitObjectType)0x8B;

            double time = Convert.ToDouble(split[2], NumberFormatInfo.InvariantInfo);
            SampleInfo section = beatmap.ControlPointAt(time).Sample;

            OsuHitObject result;

            switch (type)
            {
                case OsuHitObject.HitObjectType.Circle:
                    result = new HitCircle();
                    switch (split.Length)
                    {
                        case 6:
                            result.Sample = ParseHitSample(section, split[4], split[5]);
                            break;
                        case 5:
                            result.Sample = ParseHitSample(section, split[4], null);
                            break;
                        default:
                            throw new ArgumentException("HitCircles must have between 5 and 6 values");
                    }
                    break;
                case OsuHitObject.HitObjectType.Slider:
                    Slider s = new Slider();
                    switch (split.Length)
                    {
                        case 11:
                            s.Sample = ParseHitSample(section, split[4], split[10]);
                            break;
                        case 10:
                            s.Sample = ParseHitSample(section, split[4], null);
                            break;
                        default:
                            throw new ArgumentException("Sliders must have between 10 and 11 values");
                    }

                    CurveTypes curveType = CurveTypes.Catmull;
                    double length = 0;
                    List<Vector2> points = new List<Vector2>();
                    List<HitSampleInfo> edgeSamples = new List<HitSampleInfo>();

                    points.Add(new Vector2(Convert.ToInt32(split[0], NumberFormatInfo.InvariantInfo), Convert.ToInt32(split[1], NumberFormatInfo.InvariantInfo)));

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
                            (int)Convert.ToDouble(temp[0], NumberFormatInfo.InvariantInfo),
                            (int)Convert.ToDouble(temp[1], NumberFormatInfo.InvariantInfo)
                        );
                        points.Add(v);
                    }

                    length = Convert.ToDouble(split[7], NumberFormatInfo.InvariantInfo);

                    string[] sampleSplit = split[8].Split('|');
                    string[] additionSplit = split[9].Split('|');
                    if (sampleSplit.Length != additionSplit.Length)
                        throw new ArgumentException("Sliders must have the same amount of samples and additions");
                    for (int i = 0; i < sampleSplit.Length; i++)
                        edgeSamples.Add(ParseHitSample(section, sampleSplit[i], additionSplit[i]));

                    s.RepeatCount = Convert.ToInt32(split[6], NumberFormatInfo.InvariantInfo);
                    s.EdgeSamples = edgeSamples;

                    s.Curve = new SliderCurve
                    {
                        ControlPoints = points,
                        Length = length,
                        CurveType = curveType
                    };

                    s.Curve.Calculate();

                    result = s;
                    break;
                case OsuHitObject.HitObjectType.Spinner:
                    result = new Spinner();
                    switch (split.Length)
                    {
                        case 7:
                            result.Sample = ParseHitSample(section, split[4], split[6]);
                            break;
                        case 6:
                            result.Sample = ParseHitSample(section, split[4], null);
                            break;
                        default:
                            throw new ArgumentException("Spinners must have between 6 and 7 values");
                    }
                    break;
                case OsuHitObject.HitObjectType.Hold:
                    throw new NotImplementedException();
                default:
                    throw new UnknownHitObjectException((int)type);
            }

            result.Position = new Vector2(Convert.ToInt32(split[0], NumberFormatInfo.InvariantInfo), Convert.ToInt32(split[1], NumberFormatInfo.InvariantInfo));
            result.StartTime = time;
            result.NewCombo = combo;
            result.ComboColourOffset = comboColourOffset;
            return result;
        }
    }
}
