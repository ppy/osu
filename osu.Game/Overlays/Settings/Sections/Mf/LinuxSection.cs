using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public partial class LinuxSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Brands.Linux
        };

        public override LocalisableString Header => "Linux";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, OsuConfigManager osuConfig, GameHost host)
        {
            Add(new LinuxSettings());
            Add(new DBusSettings());
        }
    }
}
