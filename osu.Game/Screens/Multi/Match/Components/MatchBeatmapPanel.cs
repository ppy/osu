// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class MatchBeatmapPanel : MultiplayerComposite
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private CancellationTokenSource loadCancellation;
        private GetBeatmapSetRequest request;
        private DirectGridPanel panel;

        public MatchBeatmapPanel()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Playlist.ItemsAdded += _ => loadNewPanel();
            Playlist.ItemsRemoved += _ => loadNewPanel();

            loadNewPanel();
        }

        private void loadNewPanel()
        {
            loadCancellation?.Cancel();
            request?.Cancel();

            panel?.FadeOut(200);
            panel?.Expire();
            panel = null;

            var beatmap = Playlist.FirstOrDefault()?.Beatmap.Value;

            if (beatmap?.OnlineBeatmapID == null)
                return;

            loadCancellation = new CancellationTokenSource();

            request = new GetBeatmapSetRequest(beatmap.OnlineBeatmapID.Value, BeatmapSetLookupType.BeatmapId);
            request.Success += res => Schedule(() =>
            {
                panel = new DirectGridPanel(res.ToBeatmapSet(rulesets));
                LoadComponentAsync(panel, AddInternal, loadCancellation.Token);
            });

            api.Queue(request);
        }
    }
}
