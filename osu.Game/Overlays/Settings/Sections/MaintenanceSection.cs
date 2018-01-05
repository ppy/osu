// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.Maintenance;
using OpenTK;

namespace osu.Game.Overlays.Settings.Sections
{
    public class MaintenanceSection : SettingsSection
    {
        public override string Header => "Maintenance";
        public override FontAwesome Icon => FontAwesome.fa_wrench;

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
