// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public interface IReadFromConfig
    {
        void ReadFromConfig(OsuConfigManager config);
    }
}
