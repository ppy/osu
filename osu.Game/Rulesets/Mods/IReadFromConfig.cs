using osu.Framework.Allocation;
using osu.Game.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.Mods
{
    public interface IReadFromConfig
    {
        void ApplyToConfig(OsuConfigManager config);
    }
}
