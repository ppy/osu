// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A special type of HitObject, mostly used for legacy conversion of "holds".
    /// </summary>
    public interface IHasHold
    {
        /// <summary>
        /// The time at which the hold ends.
        /// </summary>
        double EndTime { get; }
    }
}
