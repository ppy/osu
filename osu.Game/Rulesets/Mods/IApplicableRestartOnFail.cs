// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Represents a mod which can request to restart on fail.
    /// </summary>
    public interface IApplicableRestartOnFail : IApplicableMod
    {
        /// <summary>
        /// Whether we allow restarting
        /// </summary>
        bool AllowRestart { get; }
    }
}
