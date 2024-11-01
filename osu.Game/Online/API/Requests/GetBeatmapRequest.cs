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
        public readonly int OnlineID;
        public readonly string? MD5Hash;
        public readonly string? Filename;

        public GetBeatmapRequest(IBeatmapInfo beatmapInfo)
            : this(onlineId: beatmapInfo.OnlineID, md5Hash: beatmapInfo.MD5Hash, filename: (beatmapInfo as BeatmapInfo)?.Path)
        {
        }

        public GetBeatmapRequest(int onlineId = -1, string? md5Hash = null, string? filename = null)
        {
            OnlineID = onlineId;
            MD5Hash = md5Hash;
            Filename = filename;
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
