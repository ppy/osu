// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapRequest : APIRequest<APIBeatmap>
    {
        public readonly IBeatmapInfo BeatmapInfo;
        public readonly string Filename;

        public GetBeatmapRequest(IBeatmapInfo beatmapInfo)
        {
            BeatmapInfo = beatmapInfo;
            Filename = (beatmapInfo as BeatmapInfo)?.Path ?? string.Empty;
        }

        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();

            if (BeatmapInfo.OnlineID > 0)
                request.AddParameter(@"id", BeatmapInfo.OnlineID.ToString());
            if (!string.IsNullOrEmpty(BeatmapInfo.MD5Hash))
                request.AddParameter(@"checksum", BeatmapInfo.MD5Hash);
            if (!string.IsNullOrEmpty(Filename))
                request.AddParameter(@"filename", Filename);

            return request;
        }

        protected override string Target => @"beatmaps/lookup";
    }
}
