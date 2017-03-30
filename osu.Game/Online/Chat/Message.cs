// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Newtonsoft.Json;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class Message
    {
        [JsonProperty(@"message_id")]
        public long Id;

        //todo: this should be inside sender.
        [JsonProperty(@"user_id")]
        public int UserId;

        [JsonProperty(@"channel_id")]
        public int ChannelId;

        [JsonProperty(@"timestamp")]
        public DateTimeOffset Timestamp;

        [JsonProperty(@"content")]
        public string Content;

        [JsonProperty(@"sender")]
        public User User;

        [JsonConstructor]
        public Message()
        {
        }
    }
}
