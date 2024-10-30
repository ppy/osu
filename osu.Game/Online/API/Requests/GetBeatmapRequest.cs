// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.IO.Network;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapRequest : APIRequest<APIBeatmap>
    {
        public readonly int OnlineID = -1;
        public readonly string? MD5Hash;
        public readonly string? Filename;

        public GetBeatmapRequest(IBeatmapInfo beatmapInfo)
        {
            OnlineID = beatmapInfo.OnlineID;
            MD5Hash = beatmapInfo.MD5Hash;
            Filename = (beatmapInfo as BeatmapInfo)?.Path ?? string.Empty;
        }

        public GetBeatmapRequest(string md5Hash)
        {
            MD5Hash = md5Hash;
        }

        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();

            if (OnlineID > 0)
                request.AddParameter(@"id", OnlineID.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(MD5Hash))
                request.AddParameter(@"checksum", MD5Hash);
            if (!string.IsNullOrEmpty(Filename))
                request.AddParameter(@"filename", Filename);

            return request;
        }

        protected override string Target => @"beatmaps/lookup";
    }
}
