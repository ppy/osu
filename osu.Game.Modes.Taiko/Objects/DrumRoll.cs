using OpenTK;
using osu.Game.Modes.Osu.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects
{
    public class DrumRoll : TaikoHitObject
    {
        public double Length;
        public int RepeatCount = 1;
    }
}
