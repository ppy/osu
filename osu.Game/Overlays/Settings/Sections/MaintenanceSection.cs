// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings.Sections.Maintenance;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections
{
    public class MaintenanceSection : SettingsSection
    {
        public override string Header => "Maintenance";
        public override IconUsage Icon => FontAwesome.Solid.Wrench;

        public MaintenanceSection()
        {
            FlowContent.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new GeneralSettings()
            };
        }
    }
}
