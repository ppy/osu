// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
