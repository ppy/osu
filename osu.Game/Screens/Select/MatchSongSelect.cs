// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Screens.Multi.Screens;

namespace osu.Game.Screens.Select
{
    public class MatchSongSelect : SongSelect, IMultiplayerScreen
    {
        public string ShortTitle => "song selection";

        protected override bool OnStart()
        {
            if (IsCurrentScreen) Exit();
            return true;
        }
    }
}
