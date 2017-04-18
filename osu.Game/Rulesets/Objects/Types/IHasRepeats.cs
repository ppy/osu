// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that spans some length.
    /// </summary>
    public interface IHasRepeats : IHasEndTime
    {
        /// <summary>
        /// The amount of times the HitObject repeats.
        /// </summary>
        int RepeatCount { get; }
    }
}
