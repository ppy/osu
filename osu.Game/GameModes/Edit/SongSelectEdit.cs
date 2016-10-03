//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;

namespace osu.Game.GameModes.Edit
{
    class SongSelectEdit : GameModeWhiteBox
    {
        protected override IEnumerable<Type> PossibleChildren => new[] {
                typeof(Editor)
        };
    }
}
