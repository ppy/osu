// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public abstract partial class RankedPlayTestScene : MultiplayerTestScene
    {
        /// <summary>
        /// Returns 5 sample of the chosen ruleset <see cref="APIBeatmap"/>s.
        /// </summary>
        protected static APIBeatmap[] GetSampleBeatmaps(RulesetInfo ri)
        {
            switch (ri.ShortName)
            {
                case "osu!":
                {
                    using var resourceStream = TestResources.OpenResource("Requests/api-beatmaps-rankedplay.json");
                    using var reader = new StreamReader(resourceStream);
                    return JsonConvert.DeserializeObject<APIBeatmap[]>(reader.ReadToEnd())!;
                }
                case "mania":
                {
                    using var resourceStream = TestResources.OpenResource("Requests/api-beatmaps-rankedplay-mania4k.json");
                    using var reader = new StreamReader(resourceStream);
                    return JsonConvert.DeserializeObject<APIBeatmap[]>(reader.ReadToEnd())!;
                }
                default:
                {
                    using var resourceStream = TestResources.OpenResource("Requests/api-beatmaps-rankedplay.json");
                    using var reader = new StreamReader(resourceStream);
                    return JsonConvert.DeserializeObject<APIBeatmap[]>(reader.ReadToEnd())!;
                }
            }
        }

        /// <summary>
        /// A request handler that will resolve api requests to any beatmaps provided by <see cref="GetSampleBeatmaps"/>.
        /// </summary>
        public class BeatmapRequestHandler
        {
            public APIBeatmap[] APIBeatmaps;

            public BeatmapRequestHandler(RulesetInfo ri)
            {
                APIBeatmaps = GetSampleBeatmaps(ri);
            }
            public bool HandleRequest(APIRequest request)
            {
                switch (request)
                {
                    case GetBeatmapRequest beatmapRequest:
                        var beatmap = APIBeatmaps.FirstOrDefault(it => it.OnlineID == beatmapRequest.OnlineID);

                        if (beatmap != null)
                        {
                            beatmapRequest.TriggerSuccess(beatmap);
                            return true;
                        }

                        break;

                    case GetBeatmapsRequest beatmapsRequest:
                        beatmapsRequest.TriggerSuccess(new GetBeatmapsResponse
                        {
                            Beatmaps = beatmapsRequest
                                       .BeatmapIds
                                       .Select(id => APIBeatmaps.FirstOrDefault(it => it.OnlineID == id))
                                       .ToList()
                        });

                        return true;
                }

                return false;
            }
        }


        public class RevealedRankedPlayCardWithPlaylistItem : RankedPlayCardWithPlaylistItem
        {
            public RevealedRankedPlayCardWithPlaylistItem(APIBeatmap beatmap, RankedPlayCardItem? card = null)
                : base(card ?? new RankedPlayCardItem())
            {
                PlaylistItem.Value = new MultiplayerPlaylistItem(new PlaylistItem(beatmap));
            }
        }
    }
}
