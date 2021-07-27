// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapRequest : APIRequest<APIBeatmap>
    {
        private readonly BeatmapInfo beatmap;
        private readonly string beatmapHash;

        public GetBeatmapRequest(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
        }

        public GetBeatmapRequest(string beatmapHash)
        {
            this.beatmapHash = beatmapHash;
        }

        protected override string Target
        {
            get
            {
                if (beatmap == null)
                {
                    return $@"beatmaps/lookup?checksum={beatmapHash}";
                }

                return $@"beatmaps/lookup?id={beatmap.OnlineBeatmapID}&checksum={beatmap.MD5Hash}&filename={System.Uri.EscapeUriString(beatmap.Path ?? string.Empty)}";
            }
        }
    }
}
