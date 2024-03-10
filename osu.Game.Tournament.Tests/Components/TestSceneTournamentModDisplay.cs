// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneTournamentModDisplay : TournamentTestScene
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private IRulesetStore rulesets { get; set; } = null!;

        private FillFlowContainer<TournamentBeatmapPanel> fillFlow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var req = new GetBeatmapRequest(new APIBeatmap { OnlineID = 490154 });
            req.Success += success;
            api.Queue(req);

            Add(fillFlow = new FillFlowContainer<TournamentBeatmapPanel>
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Full,
                Spacing = new Vector2(10)
            });
        }

        private void success(APIBeatmap beatmap)
        {
            var ruleset = rulesets.GetRuleset(Ladder.Ruleset.Value?.OnlineID ?? -1);

            if (ruleset == null)
                return;

            var mods = ruleset.CreateInstance().AllMods;

            foreach (var mod in mods)
            {
                fillFlow.Add(new TournamentBeatmapPanel(new TournamentBeatmap(beatmap), mod.Acronym)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                });
            }
        }
    }
}
