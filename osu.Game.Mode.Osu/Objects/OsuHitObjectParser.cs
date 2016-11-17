using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var type = (OsuBaseHit.HitObjectType)int.Parse(split[3]);
            bool combo = type.HasFlag(OsuBaseHit.HitObjectType.NewCombo);
            type &= (OsuBaseHit.HitObjectType)0xF;
            type &= ~OsuBaseHit.HitObjectType.NewCombo;
            OsuBaseHit result;
            switch (type)
            {
                case OsuBaseHit.HitObjectType.Circle:
                    result = new HitCircle();
                    break;
                case OsuBaseHit.HitObjectType.Slider:
                    Slider s = new Slider();

                    CurveTypes curveType = CurveTypes.Catmull;
                    int repeatCount = 0;
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

                    s.RepeatCount = repeatCount;

                    s.Curve = new SliderCurve
                    {
                        Path = points,
                        Length = length,
                        CurveType = curveType
                    };

                    s.Curve.Calculate();

                    result = s;
                    break;
                case OsuBaseHit.HitObjectType.Spinner:
                    result = new Spinner();
                    break;
                default:
                    //throw new InvalidOperationException($@"Unknown hit object type {type}");
                    return null;
            }
            result.Position = new Vector2(int.Parse(split[0]), int.Parse(split[1]));
            result.StartTime = double.Parse(split[2]);
            result.Sample = new HitSampleInfo { Type = (SampleType)int.Parse(split[4]) };
            result.NewCombo = combo;
            // TODO: "addition" field
            return result;
        }
    }
}
