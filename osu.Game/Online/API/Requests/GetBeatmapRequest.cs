// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;

#nullable enable

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapRequest : APIRequest<APIBeatmap>
    {
        private readonly IBeatmapInfo beatmapInfo;

        private readonly string filename;

        public GetBeatmapRequest(IBeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;

            filename = (beatmapInfo as BeatmapInfo)?.Path ?? string.Empty;
        }

        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();

            request.AddParameter(@"id", beatmapInfo.OnlineID.ToString());
            request.AddParameter(@"checksum", beatmapInfo.MD5Hash);
            request.AddParameter(@"filename", filename);

            return request;
        }

        protected override string Target => @"beatmaps/lookup";
    }
}
