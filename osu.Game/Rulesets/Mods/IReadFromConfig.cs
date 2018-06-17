// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that require reading access to the osu! configuration.
    /// </summary>
    public interface IReadFromConfig
    {
        void ReadFromConfig(OsuConfigManager config);
    }
}
