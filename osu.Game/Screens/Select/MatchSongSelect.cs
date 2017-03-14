// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Screens.Select
{
    internal class MatchSongSelect : SongSelect
    {
        public MatchSongSelect() : base(true) { }
        protected override void OnSelected() => Exit();
    }
}
