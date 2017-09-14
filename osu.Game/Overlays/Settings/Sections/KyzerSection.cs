using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.Kyzer;

namespace osu.Game.Overlays.Settings.Sections
{
    public class KyzerSection : SettingsSection
    {
        public override string Header => "Maintenance";
        public override FontAwesome Icon => FontAwesome.fa_wrench;

        public KyzerSection()
        {
            FlowContent.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new KyzerSettings()
            };
        }
    }
}
