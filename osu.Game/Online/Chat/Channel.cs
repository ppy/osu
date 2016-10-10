//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Online.Chat
{
    public class Channel
    {
        [JsonProperty(@"name")]
        public string Name;

        [JsonProperty(@"description")]
        public string Topic;

        [JsonProperty(@"type")]
        public string Type;

        [JsonProperty(@"channel_id")]
        public int Id;

        public List<Message> Messages = new List<Message>();

        //internal bool Joined;

        [JsonConstructor]
        public Channel()
        {
        }

        public event Action<Message[]> NewMessagesArrived;

        public void AddNewMessages(params Message[] messages)
        {
            Messages.AddRange(messages);
            purgeOldMessages();

            NewMessagesArrived?.Invoke(messages);
        }

        private void purgeOldMessages()
        {
            const int max_history = 50;

            int messageCount = Messages.Count;
            if (messageCount > 50)
            {
                Messages.RemoveRange(0, messageCount - max_history);
            }
        }
    }
}
