// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [Ignore("getting CI working")]
    public class TestCaseSpinner : OsuTestCase
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;

        public TestCaseSpinner()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Single", addSingle);
        }

        private void addSingle()
        {
            var spinner = new Spinner { StartTime = Time.Current + 1000, EndTime = Time.Current + 4000 };

            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 0 });

            var drawable = new DrawableSpinner(spinner)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            };

            Add(drawable);
        }
    }
}
