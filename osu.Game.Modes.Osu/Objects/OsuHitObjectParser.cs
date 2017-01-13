//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Globalization;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects;

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

                    List<HitSampleInfo> edgeSamples = new List<HitSampleInfo>();

                    switch (split.Length)
                    {
                        case 11:
                            s.Sample = ParseHitSample(section, split[4], split[10]);
                            break;
                        case 10:
                            s.Sample = ParseHitSample(section, split[4], null);
                            break;
                        case 8:
                            s.RepeatCount = Convert.ToInt32(split[6], NumberFormatInfo.InvariantInfo);
                            for (int i = 0; i <= s.RepeatCount; i++)
                                edgeSamples.Add(ParseHitSample(section, null, null));
                            goto case 10;
                        default:
                            throw new ArgumentException("Sliders must have 8, 10 or 11 values");
                    }

                    if (edgeSamples.Count == 0)
                    {
                        s.RepeatCount = Convert.ToInt32(split[6], NumberFormatInfo.InvariantInfo);

                        string[] sampleSplit = split[8].Split('|');
                        string[] additionSplit = split[9].Split('|');
                        if (sampleSplit.Length != additionSplit.Length || sampleSplit.Length != s.RepeatCount + 1)
                            throw new ArgumentException("Sliders must have the same amount of samples, additions and points");
                        for (int i = 0; i <= s.RepeatCount; i++)
                            edgeSamples.Add(ParseHitSample(section, sampleSplit[i], additionSplit[i]));
                    }

                    CurveTypes curveType = CurveTypes.Catmull;
                    double length = 0;
                    List<Vector2> points = new List<Vector2>();

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
                    combo = false;
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
