// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using System.Linq;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : Container
    {
        private readonly FillFlowContainer flow;
        private APIAccess api;

        private BeatmapInfo beatmap;
        public BeatmapInfo Beatmap
        {
            set
            {
                if (beatmap == value) return;
                beatmap = value;

                getScores();
            }
            get { return beatmap; }
        }

        public ScoresContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Child = flow = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Width = 0.95f,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 1),
            };
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        private void getScores()
        {
            flow.Clear();

            var req = new GetScoresRequest(beatmap);
            req.Success += scores =>
            {
                int i = 0;
                foreach(var s in scores.Scores)
                {
                    flow.Add(new DrawableScore(i, s));
                    i++;
                }
            };
            api.Queue(req);
        }
    }
}
