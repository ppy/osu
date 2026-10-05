// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.Json.Serialization;
using osu.Framework.Extensions.TypeExtensions;

namespace osu.Game.IPC.Messages
{
    [Serializable]
    public abstract class OsuWebSocketMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; }

        protected OsuWebSocketMessage()
        {
            Type = GetType().ReadableName();
        }
    }
}
