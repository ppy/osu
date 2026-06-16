// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Framework.Extensions.TypeExtensions;

namespace osu.Desktop.IPC.Messages
{
    public abstract class OsuWebSocketMessage
    {
        [JsonProperty("type")]
        public string Type { get; }

        protected OsuWebSocketMessage()
        {
            Type = GetType().ReadableName();
        }
    }
}
