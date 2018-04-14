// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Online.API.Requests
{
    public class GetPrivateMessagesRequest : APIMessagesRequest
    {
        private long? since;

        public GetPrivateMessagesRequest(long? sinceId = null)
            : base(sinceId)
        {
        }

        protected override string Target => @"chat/messages/private";
    }
}
