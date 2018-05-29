// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class Match : MultiplayerScreen
    {
        private readonly Room room;

        public override string Title => room.Name.Value;

        public Match(Room room)
        {
            this.room = room;
        }
    }
}
