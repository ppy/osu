// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class GetUpdatesResponse
    {
        [JsonProperty]
        public List<Channel> Presence;

        [JsonProperty]
        public List<Message> Messages;
    }
}
