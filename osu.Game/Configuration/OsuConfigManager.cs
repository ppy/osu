//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT License - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;

namespace osu.Game.Configuration
{
    class OsuConfigManager : ConfigManager<OsuConfig>
    {
        protected override void InitialiseDefaults()
        {
            Set(OsuConfig.Width, 1366);
            Set(OsuConfig.Height, 768);
            Set(OsuConfig.MouseSensitivity, 1.0);
        }
    }

    enum OsuConfig
    {
        Width,
        Height,
        MouseSensitivity,
    }
}
