// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.Debug;

namespace osu.Game.Overlays.Settings.Sections
{
    public class DebugSection : SettingsSection
    {
        public override string Header => "Debug";
        public override FontAwesome Icon => FontAwesome.fa_bug;

        public DebugSection()
        {
            Children = new Drawable[]
            {
                new GeneralSettings(),
                new GCSettings(),
            };
        }
    }
}
