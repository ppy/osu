using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class LinuxSettings : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Brands.Linux
        };

        public override LocalisableString Header => "Linux";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, OsuConfigManager osuConfig, GameHost host)
        {
            AddRange(new Drawable[]
            {
                new SettingsEnumDropdown<GamemodeActivateCondition>
                {
                    LabelText = "Gamemode启用条件",
                    TooltipText = "依赖libgamemode",
                    Current = config.GetBindable<GamemodeActivateCondition>(MSetting.Gamemode)
                },
                new SettingsCheckbox
                {
                    LabelText = "DBus集成",
                    Current = config.GetBindable<bool>(MSetting.DBusIntegration)
                }
            });
        }
    }
}
