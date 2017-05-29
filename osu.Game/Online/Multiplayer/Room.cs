// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Game.Database;
using osu.Game.Users;

namespace osu.Game.Online.Multiplayer
{
    public class Room
    {
        public Bindable<string> Name = new Bindable<string>();
        public Bindable<User> Host = new Bindable<User>();
        public Bindable<RoomStatus> Status = new Bindable<RoomStatus>();
        public Bindable<BeatmapMetadata> Beatmap = new Bindable<BeatmapMetadata>();
    }
}
