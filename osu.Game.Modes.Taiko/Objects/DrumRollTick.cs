// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
