// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Screens.Select.Details;
using System;
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
                },
            });

            AddStep("new retry/fail values", newRetryAndFailValues);
            AddStep("new ratings", newRatings);
        }

        private int lastRange = 1;

        private void newRetryAndFailValues()
        {
            details.Fails = Enumerable.Range(lastRange, 100).Select(i => (int)(Math.Cos(i) * 100));
            details.Retries = Enumerable.Range(lastRange, 100).Select(i => (int)(Math.Sin(i) * 100));
            lastRange += 100;
        }

        private void newRatings()
        {
            details.Ratings = Enumerable.Range(1, 10);
        }
    }
}
