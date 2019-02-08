﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class CreateNewPrivateMessageResponse
    {
        [JsonProperty("new_channel_id")]
        public int ChannelID;

        public Message Message;
    }
}
