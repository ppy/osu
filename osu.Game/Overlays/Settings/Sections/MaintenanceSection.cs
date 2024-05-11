// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.Maintenance;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class MaintenanceSection : SettingsSection
    {
        public override LocalisableString Header => MaintenanceSettingsStrings.MaintenanceSectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = OsuIcon.Maintenance
        };

        public MaintenanceSection()
        {
            Children = new Drawable[]
            {
                new BeatmapSettings(),
                new SkinSettings(),
                new CollectionsSettings(),
                new ScoreSettings(),
                new ModPresetSettings()
            };
        }
    }
}
