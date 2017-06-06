// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Configuration;
using osu.Framework.Lists;

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

        public readonly SortedList<Message> Messages = new SortedList<Message>(Comparer<Message>.Default);

        public Bindable<bool> Joined = new Bindable<bool>();

        public bool ReadOnly => Name != "#lazer";

        public const int MAX_HISTORY = 300;

        [JsonConstructor]
        public Channel()
        {
        }

        public event Action<IEnumerable<Message>> NewMessagesArrived;

        public void AddNewMessages(params Message[] messages)
        {
            messages = messages.Except(Messages).ToArray();

            Messages.AddRange(messages);

            purgeOldMessages();

            NewMessagesArrived?.Invoke(messages);
        }

        private void purgeOldMessages()
        {
            int messageCount = Messages.Count;
            if (messageCount > MAX_HISTORY)
                Messages.RemoveRange(0, messageCount - MAX_HISTORY);
        }

        public override string ToString() => Name;
    }
}
