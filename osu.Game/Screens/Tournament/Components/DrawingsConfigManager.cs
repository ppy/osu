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
#pragma warning disable CS0612 // Type or member is obsolete
            Set(DrawingsConfig.Groups, 8, 1, 8);
            Set(DrawingsConfig.TeamsPerGroup, 8, 1, 8);
#pragma warning restore CS0612 // Type or member is obsolete
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
