// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Transforms;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailArea : Container
    {
        private const float transition_duration = 500;

        private Container content;
        protected override Container<Drawable> Content => content;

        public readonly Container Details; //todo: replace with a real details view when added
        public readonly Leaderboard Leaderboard;

        private OsuGame game;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGame game)
        {
            this.game = game;
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
                                Details.FadeIn(transition_duration, EasingTypes.OutQuint);
                                Leaderboard.FadeOut(transition_duration, EasingTypes.OutQuint);
                                break;

                            default:
                                Details.FadeOut(transition_duration, EasingTypes.OutQuint);
                                Leaderboard.FadeIn(transition_duration, EasingTypes.OutQuint);
                                break;
                        }
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

        private GetScoresRequest getScoresRequest;
        public void PresentScores(WorkingBeatmap beatmap)
        {
            if (game == null) return;

            Leaderboard.Scores = null;
            getScoresRequest?.Cancel();

            if (beatmap?.BeatmapInfo == null) return;

            getScoresRequest = new GetScoresRequest(beatmap.BeatmapInfo);
            getScoresRequest.Success += r => Leaderboard.Scores = r.Scores;
            game.API.Queue(getScoresRequest);
        }
    }
}
