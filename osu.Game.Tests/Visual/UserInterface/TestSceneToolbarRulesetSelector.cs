// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Toolbar;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.MathUtils;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneToolbarRulesetSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ToolbarRulesetSelector),
            typeof(ToolbarRulesetTabButton),
        };

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Test]
        public void TestDisplay()
        {
            ToolbarRulesetSelector selector = null;

            AddStep("create selector", () =>
            {
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.X,
                    Height = Toolbar.HEIGHT,
                    Child = selector = new ToolbarRulesetSelector()
                };
            });

            AddStep("Select random", () =>
            {
                selector.Current.Value = selector.Items.ElementAt(RNG.Next(selector.Items.Count()));
            });
            AddStep("Toggle disabled state", () => selector.Current.Disabled = !selector.Current.Disabled);
        }

        [Test]
        public void TestNonFirstRulesetInitialState()
        {
            TestSelector selector = null;

            AddStep("create selector", () =>
            {
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.X,
                    Height = Toolbar.HEIGHT,
                    Child = selector = new TestSelector()
                };

                selector.Current.Value = rulesets.GetRuleset(2);
            });

            AddAssert("mode line has moved", () => selector.ModeButtonLine.DrawPosition.X > 0);
        }

        private class TestSelector : ToolbarRulesetSelector
        {
            public new Drawable ModeButtonLine => base.ModeButtonLine;
        }
    }
}
