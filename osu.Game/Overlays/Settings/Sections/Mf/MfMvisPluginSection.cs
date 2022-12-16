using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public partial class MfMvisPluginSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Plug
        };

        [BackgroundDependencyLoader]
        private void load(LLinPluginManager manager)
        {
            foreach (var pl in manager.GetAllPlugins(false))
            {
#pragma warning disable CS0618
                var legacyPage = pl.CreateSettingsSubSection();
#pragma warning restore CS0618

                if (legacyPage != null)
                {
                    Add(legacyPage);
                    continue;
                }

                if (manager.GetSettingsFor(pl)?.Length > 0)
                    Add(new PluginSettingsSubsection(pl));
            }
        }

        public override LocalisableString Header => "LLin播放器 - 设置和插件";
    }
}
