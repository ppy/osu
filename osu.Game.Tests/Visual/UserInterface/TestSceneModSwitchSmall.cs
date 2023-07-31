// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneModSwitchSmall : OsuTestScene
    {
        [Test]
        public void TestOsu() => createSwitchTestFor(new OsuRuleset());

        [Test]
        public void TestTaiko() => createSwitchTestFor(new TaikoRuleset());

        [Test]
        public void TestCatch() => createSwitchTestFor(new CatchRuleset());

        [Test]
        public void TestMania() => createSwitchTestFor(new ManiaRuleset());

        private void createSwitchTestFor(Ruleset ruleset)
        {
            AddStep("no colour scheme", () => Child = createContent(ruleset, null));

            foreach (var scheme in Enum.GetValues(typeof(OverlayColourScheme)).Cast<OverlayColourScheme>())
            {
                AddStep($"{scheme} colour scheme", () => Child = createContent(ruleset, scheme));
            }

            AddToggleStep("toggle active", active => this.ChildrenOfType<ModSwitchTiny>().ForEach(s => s.Active.Value = active));
        }

        private static Drawable createContent(Ruleset ruleset, OverlayColourScheme? colourScheme)
        {
            var switchFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Padding = new MarginPadding(20),
                ChildrenEnumerable = ruleset.CreateAllMods()
                                            .GroupBy(mod => mod.Type)
                                            .Select(group => new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Full,
                                                Spacing = new Vector2(5),
                                                ChildrenEnumerable = group.Select(mod => new ModSwitchSmall(mod))
                                            })
            };

            if (colourScheme != null)
            {
                return new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new (Type, object)[]
                    {
                        (typeof(OverlayColourProvider), new OverlayColourProvider(colourScheme.Value))
                    },
                    Child = switchFlow
                };
            }

            return switchFlow;
        }
    }
}
