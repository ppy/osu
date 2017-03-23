// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Modes.Taiko.Objects
{
    public class DrumRollTick : TaikoHitObject
    {
        /// <summary>
        /// Whether this is the first (initial) tick of the slider.
        /// </summary>
        public bool FirstTick;

        /// <summary>
        /// The distance between this tick and the next in milliseconds.
        /// <para>Half of this value is the hit window of the tick.</para>
        /// </summary>
        public double TickTimeDistance;
    }
}