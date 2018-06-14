// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Screens.Select
{
    public class MatchSongSelect : SongSelect
    {
        protected override bool OnStart()
        {
            Schedule(() =>
            {
                // needs to be scheduled else we enter an infinite feedback loop.
                if (IsCurrentScreen) Exit();
            });

            return true;
        }
    }
}
