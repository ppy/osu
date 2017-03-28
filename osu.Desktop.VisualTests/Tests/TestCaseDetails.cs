// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Screens.Testing;
using osu.Game.Database;
using osu.Game.Screens.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseDetails : TestCase
    {

        public override void Reset()
        {
            base.Reset();

            Add(new Details
            {
                RelativeSizeAxes = Axes.Both,
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
                Ratings = new[]
                {
                    1,2,3,4,5,6,7,8,9,10
                }
            });
        }
    }
}
