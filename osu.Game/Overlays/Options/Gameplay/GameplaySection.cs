//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.Gameplay
{
    public class GameplaySection : OptionsSection
    {
        public override string Header => "Gameplay";
        public override FontAwesome Icon => FontAwesome.fa_circle_o;

        public GameplaySection()
        {
            Children = new Drawable[]
            {
                new GeneralGameplayOptions(),
                new SongSelectGameplayOptions(),
            };
        }
    }
}