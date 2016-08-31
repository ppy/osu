//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.IO.Network;
using osu.Game.Online.Social;

namespace osu.Game.Online.API.Requests
{
    internal class GetMessagesRequest : APIRequest<List<Message>>
    {
        List<Channel> channels;
        long? since;

        public GetMessagesRequest(List<Channel> channels, long? sinceId)
        {
            this.channels = channels;
            this.since = sinceId;
        }

        protected override WebRequest CreateWebRequest()
        {
            string channelString = string.Empty;
            foreach (Channel c in channels)
                channelString += c.Id + ",";
            channelString = channelString.TrimEnd(',');

            var req = base.CreateWebRequest();
            req.AddParameter(@"channels", channelString);
            if (since.HasValue) req.AddParameter(@"since", since.Value.ToString());

            return req;
        }

        protected override string Target => @"chat/messages";
    }
}