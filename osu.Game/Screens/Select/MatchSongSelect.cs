﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;

namespace osu.Game.Screens.Select
{
    public class MatchSongSelect : SongSelect
    {
        protected override void OnSelected(InputState state) => Exit();
    }
}
