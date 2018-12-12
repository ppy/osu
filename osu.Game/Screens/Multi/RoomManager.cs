// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Multi
{
    public class RoomManager : PollingComponent
    {
        public IBindableCollection<Room> Rooms => rooms;
        private readonly BindableCollection<Room> rooms = new BindableCollection<Room>();

        public readonly Bindable<Room> Current = new Bindable<Room>();

        [Resolved]
        private APIAccess api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public RoomManager()
        {
            TimeBetweenPolls = 5000;
        }

        public void CreateRoom(Room room)
        {
            room.Host.Value = api.LocalUser;

            var req = new CreateRoomRequest(room);

            req.Success += result => addRoom(room, result);
            req.Failure += exception => Logger.Log($"Failed to create room: {exception}");
            api.Queue(req);
        }

        protected override Task Poll()
        {
            if (!api.IsLoggedIn)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            var pollReq = new GetRoomsRequest();

            pollReq.Success += result =>
            {
                foreach (var r in result)
                {
                    foreach (var pi in r.Playlist)
                    {
                        pi.Ruleset = rulesets.GetRuleset(pi.RulesetID);
                        pi.SetRulesets(rulesets);
                    }

                    var existing = rooms.FirstOrDefault(e => e.RoomID.Value == r.RoomID.Value);
                    if (existing == null)
                        rooms.Add(r);
                    else
                        existing.CopyFrom(r);
                }

                tcs.SetResult(true);
            };

            pollReq.Failure += _ => tcs.SetResult(false);

            api.Queue(pollReq);

            return tcs.Task;
        }

        private void addRoom(Room local, Room remote)
        {
            local.CopyFrom(remote);

            var existing = rooms.FirstOrDefault(e => e.RoomID.Value == local.RoomID.Value);
            if (existing != null)
                rooms.Remove(existing);
            rooms.Add(local);
        }

        private class CreateRoomRequest : APIRequest<Room>
        {
            private readonly Room room;

            public CreateRoomRequest(Room room)
            {
                this.room = room;
            }

            protected override WebRequest CreateWebRequest()
            {
                var req = base.CreateWebRequest();

                req.ContentType = "application/json";
                req.Method = HttpMethod.Post;

                req.AddRaw(JsonConvert.SerializeObject(room));

                return req;
            }

            protected override string Target => "rooms";
        }

        private class GetRoomsRequest : APIRequest<List<Room>>
        {
            protected override string Target => "rooms";
        }
    }
}
