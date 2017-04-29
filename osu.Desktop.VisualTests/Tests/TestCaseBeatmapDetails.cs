// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Screens.Select;
using System.Linq;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseBeatmapDetails : TestCase
    {
        public override string Description => "BeatmapDetails tab of BeatmapDetailArea";

        private BeatmapDetails details;

        public override void Reset()
        {
            base.Reset();

            Add(details = new BeatmapDetails
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(150),
                Beatmap = new BeatmapInfo
                {
                    Version = "VisualTest",
                    Metadata = new BeatmapMetadata
                    {
                        Source = "Some guy",
                        Tags = "beatmap metadata example with a very very long list of tags and not much creativity",
                    },
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = 7,
                        ApproachRate = 3.5f,
                        OverallDifficulty = 5.7f,
                        DrainRate = 1,
                    },
                    StarDifficulty = 5.3f,
                    Metrics = new BeatmapMetrics
                    {
                        Ratings = Enumerable.Range(0,10),
                        Fails = Enumerable.Range(lastRange, 100).Select(i => i % 12 - 6),
                        Retries = Enumerable.Range(lastRange - 3, 100).Select(i => i % 12 - 6),
                    },
                },
            });

            AddRepeatStep("fail values", newRetryAndFailValues, 10);
        }

        private int lastRange = 1;

        private void newRetryAndFailValues()
        {
            details.Beatmap.Metrics.Fails = Enumerable.Range(lastRange, 100).Select(i => i % 12 - 6);
            details.Beatmap.Metrics.Retries = Enumerable.Range(lastRange - 3, 100).Select(i => i % 12 - 6);
            details.Beatmap = details.Beatmap;
            lastRange += 100;
        }
    }
}