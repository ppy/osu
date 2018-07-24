// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Logging;
using osu.Game.Online.API;

namespace osu.Game.Online.Chat
{
    public class IncomingMessagesHandler
    {
        public long? LastMessageId { get; private set; }

        private APIMessagesRequest getMessagesRequest;

        public Func<APIMessagesRequest> CreateMessagesRequest { set; private get; }

        public Action<List<Message>> OnNewMessages { set; private get; }

        public bool CanRequestNewMessages => getMessagesRequest == null;

        public void RequestNewMessages(IAPIProvider api)
        {
            if (!CanRequestNewMessages)
                throw new InvalidOperationException("Requesting new messages is not possible yet, because the old request is still ongoing.");

            if (OnNewMessages == null)
                throw new InvalidOperationException($"You need to set an handler for the new incoming messages ({nameof(OnNewMessages)}) first before using {nameof(RequestNewMessages)}.");

            getMessagesRequest = CreateMessagesRequest.Invoke();

            getMessagesRequest.Success += handleNewMessages;
            getMessagesRequest.Failure += exception =>
            {
                Logger.Error(exception, "Fetching messages failed.");

                //allowing new messages to be requested even after the fail.
                getMessagesRequest = null;
            };

            api.Queue(getMessagesRequest);
        }

        private void handleNewMessages(List<Message> messages)
        {

            //allowing new messages to be requested.
            getMessagesRequest = null;

            //in case of no new messages we simply do nothing.
            if (messages == null || messages.Count == 0)
                return;

            OnNewMessages.Invoke(messages);

            LastMessageId = messages.Max(m => m.Id) ?? LastMessageId;
        }

        public void CancelOngoingRequests()
        {
            getMessagesRequest?.Cancel();
        }
    }
}
