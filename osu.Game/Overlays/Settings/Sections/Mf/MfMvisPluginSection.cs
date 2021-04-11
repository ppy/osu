using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Screens.Mvis.Plugins;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MfMvisPluginSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Plug
        };

        [BackgroundDependencyLoader]
        private void load(MvisPluginManager manager)
        {
            Logger.Log("插件设置Load!");

            foreach (var pl in manager.GetAllPlugins(false))
            {
                if (pl.Flags.Contains(MvisPlugin.PluginFlags.HasConfig))
                {
                    var section = pl.CreateSettingsSubSection();
                    if (section != null)
                        Add(section);
                }
            }
        }

        public override string Header => "M-vis播放器 - 插件";
    }
}
