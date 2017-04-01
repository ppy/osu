// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Screens.Select.Details;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailArea : Container
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        public readonly BeatmapDetails Details;
        public readonly Leaderboard Leaderboard;
        private BeatmapDetailTab currentTab;

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
                if (IsLoaded)
                    if(currentTab == BeatmapDetailTab.Details)
                        Schedule(updateDetails);
                    else
                        Schedule(updateScores);
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
                                updateDetails();
                                break;
                            default:
                                Details.Hide();
                                Leaderboard.Show();
                                updateScores();
                                break;
                        }
                        currentTab = tab;
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
                Details = new BeatmapDetails
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(5),
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



        private void updateDetails()
        {
            if (!IsLoaded) return;

            if (api == null || beatmap?.BeatmapInfo == null) return;
            
            Details.Beatmap = beatmap.Beatmap.BeatmapInfo;
        }
    }
}
