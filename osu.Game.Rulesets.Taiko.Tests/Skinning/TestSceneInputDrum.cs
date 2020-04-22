// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Skinning;
using osu.Game.Rulesets.Taiko.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneInputDrum : TaikoSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(InputDrum),
            typeof(LegacyInputDrum),
        }).ToList();

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new TaikoInputManager(new RulesetInfo { ID = 1 })
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
