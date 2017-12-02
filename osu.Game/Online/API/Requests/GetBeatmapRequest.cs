using osu.Game.Beatmaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapRequest : APIRequest<BeatmapInfo>
    {
        private readonly int beatmapId;

        public GetBeatmapRequest(int beatmapId)
        {
            this.beatmapId = beatmapId;
        }

        protected override string Target => $@"beatmaps/{beatmapId}";
    }
}
