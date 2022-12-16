using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Utils
{
    /// <summary>
    /// 此功能已暂时废弃
    /// </summary>
    [Obsolete]
    public partial class CustomFontHelper : Component
    {
        public static Bindable<string> CurrentTypeface = new Bindable<string>();
        public Action OnFontChanged;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.PreferredFont, CurrentTypeface);
            CurrentTypeface.BindValueChanged(_ => OnFontChanged?.Invoke());
        }
    }
}
