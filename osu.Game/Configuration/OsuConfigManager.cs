//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.GameModes.Play;
using osu.Game.Online.API;

namespace osu.Game.Configuration
{
    class OsuConfigManager : ConfigManager<OsuConfig>
    {
        protected override void InitialiseDefaults()
        {
            Set(OsuConfig.Width, 1366, 640);
            Set(OsuConfig.Height, 768, 480);
            Set(OsuConfig.MouseSensitivity, 1.0);

            Set(OsuConfig.Username, string.Empty);
            Set(OsuConfig.Password, string.Empty);
            Set(OsuConfig.Token, string.Empty);

            Set(OsuConfig.PlayMode, PlayMode.Osu);

            Set(OsuConfig.VolumeGlobal, 0.8, 0, 1);
            Set(OsuConfig.VolumeMusic, 1.0, 0, 1);
            Set(OsuConfig.VolumeEffect, 1.0, 0, 1);
        }

        public OsuConfigManager(BasicStorage storage) : base(storage)
        {
        }
    }

    enum OsuConfig
    {
        Width,
        Height,
        MouseSensitivity,
        Username,
        Password,
        Token,
        PlayMode,
        VolumeGlobal,
        VolumeEffect,
        VolumeMusic
    }
}
