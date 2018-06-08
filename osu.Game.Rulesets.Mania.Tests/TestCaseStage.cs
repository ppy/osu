// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestCaseStage : ManiaInputTestCase
    {
        public TestCaseStage()
            : base(4)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = new Vector2(20, 0),
                Children = new[]
                {
                    createStage(ScrollingDirection.Up, ManiaAction.Key1),
                    createStage(ScrollingDirection.Down, ManiaAction.Key3)
                }
            };
        }

        private Drawable createStage(ScrollingDirection direction, ManiaAction action)
        {
            var specialAction = ManiaAction.Special1;

            return new ScrollingTestContainer(new ScrollingInfo(direction))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Child = new ManiaStage(direction, 0, new StageDefinition { Columns = 2 }, ref action, ref specialAction)
            };
        }
    }
}
