using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects
{
    public class Spinner : TaikoHitObject
    {
        public double Length;

        public override double EndTime => StartTime + Length;
    }
}
