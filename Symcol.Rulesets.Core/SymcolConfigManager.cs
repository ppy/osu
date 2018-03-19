using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace Symcol.Rulesets.Core
{
    public class SymcolConfigManager : IniConfigManager<SymcolSetting>
    {
        protected override string Filename => "symcol.ini";

        public SymcolConfigManager(Storage storage) : base(storage) { }

        protected override void InitialiseDefaults()
        {
            Set(SymcolSetting.PlayerColor, "#ffffff");
        }
    }

    public enum SymcolSetting
    {
        PlayerColor
    }
}
