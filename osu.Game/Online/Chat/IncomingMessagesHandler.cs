// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Logging;
using osu.Game.Online.API;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Handles tracking and updating of a specific message type, allowing polling and requesting of only new messages on an ongoing basis.
    /// </summary>
    public class IncomingMessagesHandler
    {
        public delegate APIMessagesRequest CreateRequestDelegate(long? lastMessageId);

        public long? LastMessageId { get; private set; }

        private APIMessagesRequest getMessagesRequest;

        private readonly CreateRequestDelegate createRequest;
        private readonly Action<List<Message>> onNewMessages;

        public bool CanRequestNewMessages => getMessagesRequest == null;

        public IncomingMessagesHandler([NotNull] CreateRequestDelegate createRequest, [NotNull] Action<List<Message>> onNewMessages)
        {
            this.createRequest = createRequest ?? throw new ArgumentNullException(nameof(createRequest));
            this.onNewMessages = onNewMessages ?? throw new ArgumentNullException(nameof(onNewMessages));
        }

        public void RequestNewMessages(IAPIProvider api)
        {
            if (!CanRequestNewMessages)
                throw new InvalidOperationException("Requesting new messages is not possible yet, because the old request is still ongoing.");

            getMessagesRequest = createRequest.Invoke(LastMessageId);
            getMessagesRequest.Success += handleNewMessages;
            getMessagesRequest.Failure += exception =>
            {
                Logger.Error(exception, "Fetching messages failed.");

                // allowing new messages to be requested even after the fail.
                getMessagesRequest = null;
            };

            api.Queue(getMessagesRequest);
        }

        private void handleNewMessages(List<Message> messages)
        {
            // allowing new messages to be requested.
            getMessagesRequest = null;

            // in case of no new messages we simply do nothing.
            if (messages == null || messages.Count == 0)
                return;

            onNewMessages.Invoke(messages);

            LastMessageId = messages.Max(m => m.Id) ?? LastMessageId;
        }

        public void CancelOngoingRequests()
        {
            getMessagesRequest?.Cancel();
        }
    }
}
