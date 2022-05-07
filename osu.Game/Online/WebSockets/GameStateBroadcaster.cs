// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.IO.Serialization;

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

        public sealed override void Broadcast()
        {
            if (!IsLoaded)
                throw new InvalidOperationException(@"Broadcaster must be loaded before any broadcasts may be sent.");

            Scheduler.AddOnce(() => server.Broadcast(this.Serialize()));
        }
    }
}
