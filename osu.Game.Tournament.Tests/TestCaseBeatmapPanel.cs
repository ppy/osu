// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseBeatmapPanel : OsuTestCase
    {
        [Resolved]
        private APIAccess api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = 1091460 });
            req.Success += success;
            api.Queue(req);
        }

        private void success(APIBeatmap apiBeatmap)
        {
            var beatmap = apiBeatmap.ToBeatmap(rulesets);
            Add(new TournamentBeatmapPanel(beatmap)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }
    }
}
