using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class DangerousZone : SettingsSection
    {
        public override string Header => "危险地带";

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.ExclamationTriangle
        };

        public DangerousZone()
        {
            Add(new ExperimentalSettings());
        }
    }
}
