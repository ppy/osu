//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.GameModes.Backgrounds;

namespace osu.Game.GameModes.Play
{
    class PlaySongSelect : GameModeWhiteBox
    {
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        protected override IEnumerable<Type> PossibleChildren => new[] {
                typeof(ModSelect),
                typeof(Player)
        };
    }
}
