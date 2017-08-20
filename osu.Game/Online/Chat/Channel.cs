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
        // Keep track of all sent messages that are not yet received back from the server. The first item of this tuple is the local echo message and the second one the received message the local echo represents.
        public readonly List<Tuple<LocalEchoMessage, Message>> SentMessages = new List<Tuple<LocalEchoMessage, Message>>();

        public Bindable<bool> Joined = new Bindable<bool>();

        public bool ReadOnly => Name != "#lazer";

        public const int MAX_HISTORY = 300;

        [JsonConstructor]
        public Channel()
        {
        }

        public event Action<IEnumerable<Message>> NewMessagesArrived;
        public event Action<IEnumerable<Message>> LocalEchoMessagesRemoved;

        public void AddNewMessages(params Message[] messages)
        {
            messages = messages.Except(Messages).ToArray();

            // A list of all tuples that contains all sent messages that are handled within this call
            List<Tuple<LocalEchoMessage, Message>> handledSentMessages = SentMessages.Where(messageTuple => messages.ToList().Contains(messageTuple.Item2)).ToList();

            // Remove all sent messages that are handled within this call from the message list
            foreach (Tuple<LocalEchoMessage, Message> handledTuple in handledSentMessages)
                Messages.Remove(handledTuple.Item1);

            foreach (Message message in messages)
            {
                // Add non echo messages to its calculated index and local echo messages to the end
                if (!(message is LocalEchoMessage))
                    Messages.Insert(Messages.Count - (SentMessages.Count - handledSentMessages.Count), message);
                else
                {
                    Messages.Add(message);
                    SentMessages.Add(new Tuple<LocalEchoMessage, Message>((LocalEchoMessage) message, null));
                }
            }

            purgeOldMessages();

            NewMessagesArrived?.Invoke(messages);

            // Exclude all handled sent messages
            SentMessages.RemoveAll(tuple => handledSentMessages.Contains(tuple));
        }

        private void purgeOldMessages()
        {
            int messageCount = Messages.Count;
            if (messageCount > MAX_HISTORY)
            {
                // Remove sent messages if they are removed from the message list
                if (SentMessages.Count > MAX_HISTORY)
                    SentMessages.RemoveRange(0, SentMessages.Count - MAX_HISTORY);

                Messages.RemoveRange(0, messageCount - MAX_HISTORY);
            }
        }

        public void RemoveLocalEchoMessage(params LocalEchoMessage[] localEchoMessages)
        {
            foreach (LocalEchoMessage localEchoMessage in localEchoMessages)
                Messages.Remove(localEchoMessage);
            SentMessages.RemoveAll(tuple => localEchoMessages.Contains(tuple.Item1));

            LocalEchoMessagesRemoved?.Invoke(localEchoMessages);
        }

        public void ReplaceLocalEchoMessage(LocalEchoMessage oldMessage, Message newMessage)
        {
            // Replace the old message's tuple with a new one containing the received message.
            int oldTupleIndex = SentMessages.FindIndex(tuple => tuple.Item1.Equals(oldMessage));
            SentMessages.RemoveAt(oldTupleIndex);
            SentMessages.Insert(oldTupleIndex, new Tuple<LocalEchoMessage, Message>(oldMessage, newMessage));
        }

        public override string ToString() => Name;
    }
}
