// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.IO.Serialization;

namespace osu.Game.Online.Broadcasts
{
    public abstract partial class Broadcaster : Component
    {
        private IBroadcastServer? server;

        [BackgroundDependencyLoader(true)]
        private void load(IBroadcastServer? server)
        {
            this.server = server;
        }

        protected void Broadcast(string type, object message)
        {
            var payload = new Payload
            {
                Type = type,
                Message = message,
            };

            server?.Broadcast(payload.Serialize()).WaitSafely();
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        private struct Payload
        {
            [JsonProperty(@"type")]
            public string Type;

            [JsonProperty(@"message")]
            public object Message;
        }
    }
}
