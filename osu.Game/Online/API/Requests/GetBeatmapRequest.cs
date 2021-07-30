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
        private readonly int? id;
        private readonly string checksum;
        private readonly string? filename;

        public GetBeatmapRequest(BeatmapInfo beatmap)
        {
            id = beatmap.OnlineBeatmapID;
            checksum = beatmap.Hash;
            filename = beatmap.Path ?? string.Empty;
        }

        public GetBeatmapRequest(string beatmapHash)
        {
            checksum = beatmapHash;
        }

        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();

            request.AddParameter(@"checksum", checksum);

            if (id != null)
                request.AddParameter(@"id", id.Value.ToString());

            if (filename != null)
                request.AddParameter(@"filename", filename);

            return request;
        }

        protected override string Target => @"beatmaps/lookup";
    }
}
