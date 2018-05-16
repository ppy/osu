// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Represents a mod which can override (and block) a fail.
    /// </summary>
    public interface IApplicableFailOverride : IApplicableMod
    {
        /// <summary>
        /// Whether we should allow failing at the current point in time.
        /// </summary>
        bool AllowFail { get; }
    }
}
