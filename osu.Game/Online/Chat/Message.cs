//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.Chat
{
    public class Message
    {
        [JsonProperty(@"message_id")]
        public long Id;

        [JsonProperty(@"user_id")]
        public string UserId;

        [JsonProperty(@"channel_id")]
        public string ChannelId;

        [JsonProperty(@"timestamp")]
        public DateTime Timestamp;

        [JsonProperty(@"content")]
        internal string Content;

        [JsonProperty(@"sender")]
        internal User User;

        [JsonConstructor]
        public Message()
        {
        }
    }
}
