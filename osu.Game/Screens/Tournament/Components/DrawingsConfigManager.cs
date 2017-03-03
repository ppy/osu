// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Screens.Tournament.Components
{
    public class DrawingsConfigManager : ConfigManager<DrawingsConfig>
    {
        public override string Filename => @"drawings.ini";

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
