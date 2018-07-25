// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    // maybe look to osu.Game.Screens.Play.SquareGraph for reference later
    public class ChangelogChart : BufferedContainer
    {
        private const float height = 100;
        private const float transition_duration = 300;

        private readonly Container container;
        private readonly Box background;
        private APIAccess api;

        public ChangelogChart()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Child = container = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = height,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        Colour = OsuColour.Gray(0),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SpriteText
                    {
                        Text = "Graph Placeholder",
                        TextSize = 28,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = OsuColour.Gray(1),
                    },
                },
            };
        }

        private bool isEmpty(APIChangelogChart changelogChart)
        {
            if (changelogChart != null)
                foreach (BuildHistory buildHistory in changelogChart.BuildHistory)
                    if (buildHistory.UserCount > 0) return false;
            return true;
        }

        private void showChart(APIChangelogChart chartInfo, string updateStreamName = null)
        {
            if (!isEmpty(chartInfo))
            {
                background.Colour = StreamColour.FromStreamName(updateStreamName);
                container.MoveToY(0, transition_duration, Easing.InOutQuad).FadeIn(transition_duration);
            }
            else
                container.MoveToY(-height, transition_duration, Easing.InOutQuad).FadeOut(transition_duration);
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        public void ShowUpdateStream(string updateStream)
        {
            var req = new GetChangelogChartRequest(updateStream);
            req.Success += res => showChart(res, updateStream);
            api.Queue(req);
        }

        public void ShowAllUpdateStreams()
        {
            var req = new GetChangelogChartRequest();
            req.Success += res => showChart(res);
            api.Queue(req);
        }
    }
}
