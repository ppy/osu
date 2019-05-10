using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class OsuMovement
    {
        public double D { get; private set; }
        public double MT { get; private set; }

        
        public OsuMovement(OsuHitObject obj0, OsuHitObject obj1, OsuHitObject obj2, double clockRate)
        {
            this.D = (obj2.Position - obj1.EndPosition).Length / (2 * obj1.Radius);
            this.MT = (obj2.StartTime - obj1.StartTime) / clockRate / 1000.0;
        }
    }
}
