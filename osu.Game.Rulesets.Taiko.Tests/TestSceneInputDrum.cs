// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osuTK;
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
    public class TestSceneInputDrum : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(InputDrum),
            typeof(DrumSampleMapping),
            typeof(HitSampleInfo),
            typeof(SampleControlPoint)
        };

        public TestSceneInputDrum()
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
