// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Configuration;

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

        public readonly List<Message> Messages = new List<Message>();
        // We need to keep track of all confirmed sent messages to replace them when they are received. This ensures the same order as the server sends them.
        public readonly List<Tuple<LocalEchoMessage, Message>> ConfirmedSentMessages = new List<Tuple<LocalEchoMessage, Message>>();

        public Bindable<bool> Joined = new Bindable<bool>();

        public bool ReadOnly => Name != "#lazer";

        public const int MAX_HISTORY = 300;

        [JsonConstructor]
        public Channel()
        {
        }

        public event Action<IEnumerable<Message>> NewMessagesArrived;
        public event Action<IEnumerable<Message>> MessagesRemoved;

        public void AddNewMessages(params Message[] messages)
        {
            messages = messages.Except(Messages).ToArray();

            // A list of all tuples that contains all confirmed sent messages that are handled within this call
            List<Tuple<LocalEchoMessage, Message>> handledConfirmedSentMessages = ConfirmedSentMessages.Where(messageTuple => messages.ToList().Contains(messageTuple.Item2)).ToList();

            // Remove all confirmed sent messages that are handled within this call
            foreach (Tuple<LocalEchoMessage, Message> handledTuple in handledConfirmedSentMessages)
                Messages.Remove(handledTuple.Item1);

            // Calculate the insertion index for all non echo messages. They are inserted in front of the first local echo message
            int nonEchoInsertionIndex = -1;
            if (messages.Any(message => !(message is LocalEchoMessage)))
                nonEchoInsertionIndex = Messages.FindIndex(element => element is LocalEchoMessage);

            foreach (Message message in messages)
            {
                // Add non echo messages to its calculated index and local echo messages to the end
                if (!(message is LocalEchoMessage) && nonEchoInsertionIndex != -1)
                    Messages.Insert(nonEchoInsertionIndex++, message);
                else
                    Messages.Add(message);
            }

            purgeOldMessages();

            NewMessagesArrived?.Invoke(messages);

            // Exclude all handled confirmed sent messages
            ConfirmedSentMessages.RemoveAll(tuple => handledConfirmedSentMessages.Contains(tuple));
        }

        private void purgeOldMessages()
        {
            int messageCount = Messages.Count;
            if (messageCount > MAX_HISTORY)
                Messages.RemoveRange(0, messageCount - MAX_HISTORY);
        }

        public void RemoveMessages(params Message[] messages)
        {
            foreach (Message message in messages)
                Messages.Remove(message);

            MessagesRemoved?.Invoke(messages);
        }

        public void ReplaceLocalEchoMessage(LocalEchoMessage oldMessage, Message newMessage) => ConfirmedSentMessages.Add(new Tuple<LocalEchoMessage, Message>(oldMessage, newMessage));

        public override string ToString() => Name;
    }
}
