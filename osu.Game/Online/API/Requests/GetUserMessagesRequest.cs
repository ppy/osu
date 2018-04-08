// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.IO.Network;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class GetUserMessagesRequest : APIRequest<List<Message>>
    {
        private long? since;

        public GetUserMessagesRequest(long? sinceId = null)
        {
            since = sinceId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();
            if (since.HasValue)
                request.AddParameter(@"since", since.Value.ToString());

            return request;
        }

        protected override string Target => @"chat/messages/private";
    }
}
