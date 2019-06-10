// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Toolbar;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using System.Linq;
using osu.Framework.MathUtils;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneToolbarRulesetSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ToolbarRulesetSelector),
            typeof(ToolbarRulesetButton),
        };

        public TestSceneToolbarRulesetSelector()
        {
            ToolbarRulesetSelector selector;

            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.X,
                Height = Toolbar.HEIGHT,
                Child = selector = new ToolbarRulesetSelector()
            });

            AddStep("Select random", () =>
            {
                selector.Current.Value = selector.Items.ElementAt(RNG.Next(selector.Items.Count()));
            });
        }
    }
}
