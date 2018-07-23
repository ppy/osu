// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Configuration;
using osu.Framework.Lists;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class Channel
    {
        public readonly int MaxHistory = 300;

        /// <summary>
        /// Contains every joined user except the current logged in user.
        /// </summary>
        public readonly ObservableCollection<User> JoinedUsers = new ObservableCollection<User>();

        public readonly SortedList<Message> Messages = new SortedList<Message>(Comparer<Message>.Default);
        private readonly List<LocalEchoMessage> pendingMessages = new List<LocalEchoMessage>();

        public event Action<IEnumerable<Message>> NewMessagesArrived;
        public event Action<LocalEchoMessage, Message> PendingMessageResolved;
        public event Action<Message> MessageRemoved;

        public readonly Bindable<bool> Joined = new Bindable<bool>();
        public TargetType Target { get; protected set; }
        public bool ReadOnly => false; //todo not yet used.
        public override string ToString() => Name;

        [JsonProperty(@"name")]
        public string Name;

        [JsonProperty(@"description")]
        public string Topic;

        [JsonProperty(@"type")]
        public string Type;

        [JsonProperty(@"channel_id")]
        public long Id;

        [JsonConstructor]
        public Channel()
        {
        }

        public void AddLocalEcho(LocalEchoMessage message)
        {
            pendingMessages.Add(message);
            Messages.Add(message);

            NewMessagesArrived?.Invoke(new[] { message });
        }

        public void AddNewMessages(params Message[] messages)
        {
            messages = messages.Except(Messages).ToArray();

            Messages.AddRange(messages);

            purgeOldMessages();

            NewMessagesArrived?.Invoke(messages);
        }

        /// <summary>
        /// Replace or remove a message from the channel.
        /// </summary>
        /// <param name="echo">The local echo message (client-side).</param>
        /// <param name="final">The response message, or null if the message became invalid.</param>
        public void ReplaceMessage(LocalEchoMessage echo, Message final)
        {
            if (!pendingMessages.Remove(echo))
                throw new InvalidOperationException("Attempted to remove echo that wasn't present");

            Messages.Remove(echo);

            if (final == null)
            {
                MessageRemoved?.Invoke(echo);
                return;
            }

            if (Messages.Contains(final))
                throw new InvalidOperationException("Attempted to add the same message again");

            Messages.Add(final);
            PendingMessageResolved?.Invoke(echo, final);
        }

        private void purgeOldMessages()
        {
            // never purge local echos
            int messageCount = Messages.Count - pendingMessages.Count;
            if (messageCount > MaxHistory)
                Messages.RemoveRange(0, messageCount - MaxHistory);
        }
    }
}
