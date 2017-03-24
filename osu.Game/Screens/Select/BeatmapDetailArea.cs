// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailArea : Container
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        public readonly Container Details; //todo: replace with a real details view when added
        public readonly Leaderboard Leaderboard;

        private APIAccess api;

        private WorkingBeatmap beatmap;
        public WorkingBeatmap Beatmap
        {
            get
            {
                return beatmap;
            }
            set
            {
                beatmap = value;
                if (IsLoaded) Schedule(updateScores);
            }
        }

        public BeatmapDetailArea()
        {
            AddInternal(new Drawable[]
            {
                new BeatmapDetailAreaTabControl
                {
                    RelativeSizeAxes = Axes.X,
                    OnFilter = (tab, mods) => 
                    {
                        switch (tab)
                        {
                            case BeatmapDetailTab.Details:
                                Details.Show();
                                Leaderboard.Hide();
                                break;
                            default:
                                Details.Hide();
                                Leaderboard.Show();
                                break;
                        }

                        //for now let's always update scores.
                        updateScores();
                    },
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = BeatmapDetailAreaTabControl.HEIGHT },
                },
            });

            Add(new Drawable[]
            {
                Details = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                Leaderboard = new Leaderboard
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateScores();
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        private GetScoresRequest getScoresRequest;
        private void updateScores()
        {
            if (!IsLoaded) return;

            Leaderboard.Scores = null;
            getScoresRequest?.Cancel();

            if (api == null || beatmap?.BeatmapInfo == null || !Leaderboard.IsPresent) return;

            getScoresRequest = new GetScoresRequest(beatmap.BeatmapInfo);
            getScoresRequest.Success += r => Leaderboard.Scores = r.Scores;
            api.Queue(getScoresRequest);
        }
    }
}
