// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using System;
using System.Collections.Generic;

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
                if (string.IsNullOrEmpty(updateStreamName))
                    plotCharts(chartInfo);
                else
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

        private List<float> clearUpDips(List<BuildHistory> buildHistories, float maxUserCount)
        {
            var buildHistory = new List<float>();
            foreach (BuildHistory build in buildHistories)
            {
                if (build.UserCount / maxUserCount > 0.2f)
                    buildHistory.Add(build.UserCount);
            }
            return buildHistory;
        }

        private void plotChart(APIChangelogChart changelogChartInfo, ColourInfo colour)
        {
            var maxUserCount = getMaxUserCount(changelogChartInfo);

            container.Child = new BarGraph
            {
                Colour = colour,
                Values = clearUpDips(changelogChartInfo.BuildHistory, maxUserCount),
                RelativeSizeAxes = Axes.Both,
                Direction = BarDirection.BottomToTop,
            };
        }

        private void plotCharts(APIChangelogChart changelogChartInfo)
        {
            var maxUserCount = getMaxUserCount(changelogChartInfo);

            var releaseStreams = new Dictionary<string, List<float>>(changelogChartInfo.Order.Count);
            var highestUserCounts = new Dictionary<string, float>(changelogChartInfo.Order.Count);

            foreach (string updateStream in changelogChartInfo.Order)
            {
                releaseStreams.Add(updateStream, new List<float>());
                highestUserCounts.Add(updateStream, 0);
            }

            foreach (BuildHistory build in changelogChartInfo.BuildHistory)
            {
                releaseStreams[build.Label].Add(build.UserCount);
                if (highestUserCounts[build.Label] < build.UserCount)
                    highestUserCounts[build.Label] = build.UserCount;
            }

            container.Clear();

            foreach (KeyValuePair<string, List<float>> releaseStream in releaseStreams)
            {
                var barGraph = new BarGraph
                {
                    Colour = StreamColour.FromStreamName(releaseStream.Key),
                    Values = releaseStream.Value,
                    RelativeSizeAxes = Axes.Both,
                    Direction = BarDirection.BottomToTop,
                    //Height = highestUserCounts[releaseStream.Key] / maxUserCount,
                };
                container.Add(barGraph);
            }
        }
    }
}
