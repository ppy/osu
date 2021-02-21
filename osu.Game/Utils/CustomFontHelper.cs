using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Utils
{
    internal class CustomFontHelper : Component
    {
        public static Bindable<string> CurrentTypeface = new Bindable<string>();

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.CurrentFont, CurrentTypeface);
        }
    }
}
