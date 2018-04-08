// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;

namespace osu.Game.Online.Chat
{
    public class ChannelChat : ChatBase
    {
        [JsonProperty(@"name")]
        public string Name;

        [JsonProperty(@"description")]
        public string Topic;

        [JsonProperty(@"type")]
        public string Type;

        [JsonProperty(@"channel_id")]
        public int Id;

        [JsonConstructor]
        public ChannelChat()
        {
        }

        public override string ToString() => Name;
        public override long ChatID => Id;
        public override TargetType Target => TargetType.Channel;
    }
}
