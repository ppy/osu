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
                Metadata = new BeatmapMetadata
                {
                    Source = "Some guy",
                    Tags = "beatmap metadata example with a very very long list of tags and not much creativity",
                },
            });
        }
    }
}
