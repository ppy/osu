using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects
{
    public class DrumRollTick : TaikoHitObject
    {
        /// <summary>
        /// Whether this is the first (initial) tick of the slider.
        /// Determines whether the tick is filled or not.
        /// </summary>
        public bool FirstTick;

        public double TickTimeDistance;

        public override TaikoHitType Type => TaikoHitType.DrumRollTick;
    }
}
