﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit;

namespace osu.Game.Screens.Select
{
    class EditSongSelect : ScreenWhiteBox
    {
        protected override IEnumerable<Type> PossibleChildren => new[] {
                typeof(Editor)
        };

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg4");
    }
}
