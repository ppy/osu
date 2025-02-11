// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.Notifications.WebSocket.Requests
{
    /// <summary>
    /// A websocket message notifying the server that the client wants to receive chat messages.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StartChatRequest : SocketMessage
    {
        public StartChatRequest()
        {
            Event = @"chat.start";
        }
    }
}
