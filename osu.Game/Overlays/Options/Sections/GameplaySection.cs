// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Options.Sections.Gameplay;

namespace osu.Game.Overlays.Options.Sections
{
    public class GameplaySection : OptionsSection
    {
        public override string Header => "Gameplay";
        public override FontAwesome Icon => FontAwesome.fa_circle_o;

        public GameplaySection()
        {
            base.Children = new Drawable[]
            {
                new GeneralOptions(),
                new SongSelectOptions(),
            };
        }
    }
}