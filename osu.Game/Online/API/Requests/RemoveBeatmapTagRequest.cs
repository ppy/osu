// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public class RemoveBeatmapTagRequest : APIRequest
    {
        public int BeatmapID { get; }
        public long TagID { get; }

        public RemoveBeatmapTagRequest(int beatmapID, long tagID)
        {
            BeatmapID = beatmapID;
            TagID = tagID;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Delete;
            return req;
        }

        protected override string Target => $@"beatmaps/{BeatmapID}/tags/{TagID}";
    }
}
