//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Game.Online.API;

namespace osu.Game.Configuration
{
    class OsuConfigManager : ConfigManager<OsuConfig>
    {
        protected override void InitialiseDefaults()
        {
            Set(OsuConfig.Width, 1366);
            Set(OsuConfig.Height, 768);
            Set(OsuConfig.Maximized, false);
            Set(OsuConfig.MouseSensitivity, 1.0);

            Set(OsuConfig.Username, string.Empty);
            Set(OsuConfig.Password, string.Empty);
            Set(OsuConfig.Token, string.Empty);
        }
    }

    enum OsuConfig
    {
        Width,
        Height,
        Maximized,
        MouseSensitivity,
        Username,
        Password,
        Token
    }
}
