// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using System;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogChart : Container
    {
        private const float height = 100;
        private const float transition_duration = 300;

        // why make the child buffered? https://streamable.com/swbdj
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
                    //background = new Box
                    //{
                    //    Colour = OsuColour.Gray(0),
                    //    RelativeSizeAxes = Axes.Both,
                    //},
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
                    if (buildHistory.UserCount > 0)
                        return false;
            return true;
        }

        private void showChart(APIChangelogChart chartInfo, string updateStreamName = null)
        {
            if (!isEmpty(chartInfo))
            {
                container.MoveToY(0, transition_duration, Easing.InOutQuad).FadeIn(transition_duration);
                plotChart(chartInfo, StreamColour.FromStreamName(updateStreamName));
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

        // this could probably be combined with isEmpty, todo
        private float getMaxUserCount(APIChangelogChart changelogChartInfo)
        {
            var maxUserCount = 0l;
            foreach (BuildHistory build in changelogChartInfo.BuildHistory)
            {
                if (build.UserCount > maxUserCount)
                    maxUserCount = build.UserCount;
            }
            return maxUserCount;
        }

        private void plotChart(APIChangelogChart changelogChartInfo, ColourInfo colour)
        {
            var maxUserCount = getMaxUserCount(changelogChartInfo);
            var currentPos = 0f;

            container.Clear();

            foreach (BuildHistory build in changelogChartInfo.BuildHistory)
            {
                container.Add(new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = Math.Max(container.DrawWidth / changelogChartInfo.BuildHistory.Count, 2),
                    Height = build.UserCount / maxUserCount,
                    X = currentPos,
                    Colour = colour,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                });
                currentPos += container.DrawWidth / changelogChartInfo.BuildHistory.Count;
            }
        }
    }
}
