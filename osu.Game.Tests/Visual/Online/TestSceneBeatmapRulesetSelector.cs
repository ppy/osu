// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapRulesetSelector : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly TestRulesetSelector selector;

        public TestSceneBeatmapRulesetSelector()
        {
            Add(selector = new TestRulesetSelector());
        }

        [Resolved]
        private IRulesetStore rulesets { get; set; }

        [Test]
        public void TestMultipleRulesetsBeatmapSet()
        {
            var enabledRulesets = rulesets.AvailableRulesets.Skip(1).Take(2);

            AddStep("load multiple rulesets beatmapset", () =>
            {
                selector.BeatmapSet = new APIBeatmapSet
                {
                    Beatmaps = enabledRulesets.Select(r => new APIBeatmap { RulesetID = r.OnlineID }).ToArray()
                };
            });

            var tabItems = selector.TabContainer.TabItems;
            AddAssert("other rulesets disabled", () => tabItems.Except(tabItems.Where(t => enabledRulesets.Any(r => r.Equals(t.Value)))).All(t => !t.Enabled.Value));
            AddAssert("left-most ruleset selected", () => tabItems.First(t => t.Enabled.Value).Active.Value);
        }

        [Test]
        public void TestSingleRulesetBeatmapSet()
        {
            var enabledRuleset = rulesets.AvailableRulesets.Last();

            AddStep("load single ruleset beatmapset", () =>
            {
                selector.BeatmapSet = new APIBeatmapSet
                {
                    Beatmaps = new[]
                    {
                        new APIBeatmap
                        {
                            RulesetID = enabledRuleset.OnlineID
                        }
                    }
                };
            });

            AddAssert("single ruleset selected", () => selector.SelectedTab.Value.Equals(enabledRuleset));
        }

        [Test]
        public void TestEmptyBeatmapSet()
        {
            AddStep("load empty beatmapset", () => selector.BeatmapSet = new APIBeatmapSet());

            AddAssert("no ruleset selected", () => selector.SelectedTab == null);
            AddAssert("all rulesets disabled", () => selector.TabContainer.TabItems.All(t => !t.Enabled.Value));
        }

        [Test]
        public void TestNullBeatmapSet()
        {
            AddStep("load null beatmapset", () => selector.BeatmapSet = null);

            AddAssert("no ruleset selected", () => selector.SelectedTab == null);
            AddAssert("all rulesets disabled", () => selector.TabContainer.TabItems.All(t => !t.Enabled.Value));
        }

        private class TestRulesetSelector : BeatmapRulesetSelector
        {
            public new TabItem<RulesetInfo> SelectedTab => base.SelectedTab;

            public new TabFillFlowContainer TabContainer => base.TabContainer;
        }
    }
}
