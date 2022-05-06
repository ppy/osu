// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Online.WebSockets
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class GameStateBroadcaster : Component
    {
        [JsonProperty]
        public abstract string Type { get; }

        public abstract void Broadcast();
    }

    public abstract class GameStateBroadcaster<T> : GameStateBroadcaster
    {
        [JsonProperty]
        public abstract T Message { get; }

        [Resolved]
        private GameStateBroadcastServer server { get; set; }
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public sealed override void Broadcast()
        {
            if (!IsLoaded)
                throw new InvalidOperationException(@"Broadcaster must be loaded before any broadcasts may be sent.");

            Scheduler.AddOnce(() => server.Broadcast(JsonConvert.SerializeObject(this, settings)));
        }
    }
}
