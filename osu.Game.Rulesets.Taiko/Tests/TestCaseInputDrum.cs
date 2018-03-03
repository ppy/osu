// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Audio;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TestCaseInputDrum : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(InputDrum),
            typeof(DrumSampleMapping),
            typeof(SampleInfo),
            typeof(SampleControlPoint)
        };

        public TestCaseInputDrum()
        {
            Add(new TaikoInputManager(new RulesetInfo { ID = 1 })
            {
                RelativeSizeAxes = Axes.Both,
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(200),
                    Child = new InputDrum(new ControlPointInfo())
                }
            });
        }
    }
}
