// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.Gameplay;

namespace osu.Game.Overlays.Settings.Sections
{
    public class GameplaySection : SettingsSection
    {
        public override string Header => "Gameplay";
        public override FontAwesome Icon => FontAwesome.fa_circle_o;

        public GameplaySection()
        {
            Children = new Drawable[]
            {
                new GeneralSettings(),
                new SongSelectSettings(),
            };
        }
    }
}