// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
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
    // placeholder json file: https://api.myjson.com/bins/10ye8a
    public class ChangelogChart : BufferedContainer
    {
        private Box background;
        private SpriteText text;
        private APIAccess api;

        public ChangelogChart()
        {
            RelativeSizeAxes = Axes.X;
            Height = 100;
            Children = new Drawable[]
            {
                background = new Box
                {
                    Colour = StreamColour.STABLE,
                    RelativeSizeAxes = Axes.Both,
                },
                text = new SpriteText
                {
                    Text = "Graph Placeholder",
                    TextSize = 28,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        public void ShowChart(APIChangelog releaseStream) => fetchAndShowChangelogChart(releaseStream);

        private bool isEmpty(APIChangelogChart changelogChart)
        {
            if (changelogChart != null)
                foreach (BuildHistory buildHistory in changelogChart.BuildHistory)
                    if (buildHistory.UserCount > 0) return false;
            return true;
        }

        private void showChart(APIChangelogChart chartInfo, string updateStreamName)
        {
            if (!isEmpty(chartInfo))
            {
                background.Colour = StreamColour.FromStreamName(updateStreamName);
                text.Text = "Graph placeholder\n(chart is not empty)";
            }
            else
            {
                background.Colour = Color4.Black;
                text.Text = "Graph placeholder\n(chart is empty)";
            }
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        private void fetchAndShowChangelogChart(APIChangelog build)
        {
            var req = new GetChangelogChartRequest(build.UpdateStream.Name);
            req.Success += res => showChart(res, build.UpdateStream.Name);
            api.Queue(req);
        }
    }
}
