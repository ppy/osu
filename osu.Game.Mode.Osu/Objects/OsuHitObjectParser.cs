using System;
using System.Collections.Generic;
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
                    result = new Slider();
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
