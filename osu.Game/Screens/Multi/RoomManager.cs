// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi
{
    public class RoomManager : Component
    {
        public IBindableCollection<Room> Rooms => rooms;
        private readonly BindableCollection<Room> rooms = new BindableCollection<Room>();

        public readonly Bindable<Room> Current = new Bindable<Room>();

        [Resolved]
        private APIAccess api { get; set; }

        public void CreateRoom(Room room)
        {
            room.Host.Value = api.LocalUser;

            // Todo: Perform API request

            room.Created.Value = true;
            rooms.Add(room);
        }
    }
}
