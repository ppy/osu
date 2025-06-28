// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.Http;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public class PinScoreRequest : APIRequest
    {
        private readonly long scoreId;

        public PinScoreRequest(long scoreId)
        {
            this.scoreId = scoreId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            return req;
        }

        protected override string Target => $@"score-pins/{scoreId}";
    }

    public class UnpinScoreRequest : APIRequest
    {
        private readonly long scoreId;

        public UnpinScoreRequest(long scoreId)
        {
            this.scoreId = scoreId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Delete;
            return req;
        }

        protected override string Target => $@"score-pins/{scoreId}";
    }

    public class ReorderPinnedScoresRequest : APIRequest
    {
        private readonly long scoreId;
        private readonly int newPosition;

        public ReorderPinnedScoresRequest(long scoreId, int newPosition)
        {
            this.scoreId = scoreId;
            this.newPosition = newPosition;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            req.AddParameter("position", newPosition.ToString());
            return req;
        }

        protected override string Target => $@"score-pins/{scoreId}/reorder";
    }
}
