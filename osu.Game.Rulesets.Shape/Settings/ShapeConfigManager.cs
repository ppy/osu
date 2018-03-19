using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Rulesets.Shape.Settings
{
    public class ShapeConfigManager : IniConfigManager<ShapeSetting>
    {
        protected override string Filename => @"shape.ini";

        public ShapeConfigManager(Storage storage) : base(storage) { }

        protected override void InitialiseDefaults()
        {
            Set(ShapeSetting.Skin, "default");
        }

    }

    public enum ShapeSetting
    {
        Skin
    }
}
