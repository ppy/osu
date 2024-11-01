// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class AddFriendRequest : APIRequest<APIRelation>
    {
        public readonly int TargetId;

        public AddFriendRequest(int targetId)
        {
            TargetId = targetId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Method = HttpMethod.Post;
            req.AddParameter("target", TargetId.ToString(), RequestParameterType.Query);

            return req;
        }

        protected override string Target => @"friends";
    }
}
