//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.GameModes.Backgrounds;

namespace osu.Game.GameModes.Edit
{
    class EditSongSelect : GameModeWhiteBox
    {
        protected override IEnumerable<Type> PossibleChildren => new[] {
                typeof(Editor)
        };

        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");
    }
}
