// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Tournament.Screens.Drawings.Components
{
    public class DrawingsConfigManager : IniConfigManager<DrawingsConfig>
    {
        protected override string Filename => @"drawings.ini";

        protected override void InitialiseDefaults()
        {
            Set(DrawingsConfig.Groups, 8, 1, 8);
            Set(DrawingsConfig.TeamsPerGroup, 8, 1, 8);
        }

        public DrawingsConfigManager(Storage storage)
            : base(storage)
        {
        }
    }

    public enum DrawingsConfig
    {
        Groups,
        TeamsPerGroup
    }
}
