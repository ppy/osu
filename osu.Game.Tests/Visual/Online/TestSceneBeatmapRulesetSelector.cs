// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneBeatmapRulesetSelector : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private BeatmapRulesetSelector selector;

        [SetUp]
        public void SetUp() => Schedule(() => Child = selector = new BeatmapRulesetSelector
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            BeatmapSet = new APIBeatmapSet(),
        });

        [Test]
        public void TestDisplay()
        {
            AddSliderStep("osu", 0, 100, 0, v => updateBeatmaps(0, v));
            AddSliderStep("taiko", 0, 100, 0, v => updateBeatmaps(1, v));
            AddSliderStep("fruits", 0, 100, 0, v => updateBeatmaps(2, v));
            AddSliderStep("mania", 0, 100, 0, v => updateBeatmaps(3, v));

            void updateBeatmaps(int ruleset, int count)
            {
                if (selector == null)
                    return;

                selector.BeatmapSet = new APIBeatmapSet
                {
                    Beatmaps = selector.BeatmapSet!.Beatmaps
                                       .Where(b => b.Ruleset.OnlineID != ruleset)
                                       .Concat(Enumerable.Range(0, count).Select(_ => new APIBeatmap { RulesetID = ruleset }))
                                       .ToArray(),
                };
            }
        }

        [Test]
        public void TestMultipleRulesetsBeatmapSet()
        {
            AddStep("load multiple rulesets beatmapset", () =>
            {
                selector.BeatmapSet = new APIBeatmapSet
                {
                    Beatmaps = new[]
                    {
                        new APIBeatmap { RulesetID = 1 },
                        new APIBeatmap { RulesetID = 2 },
                    }
                };
            });

            AddAssert("osu disabled", () => !selector.ChildrenOfType<BeatmapRulesetTabItem>().Single(t => t.Value.OnlineID == 0).Enabled.Value);
            AddAssert("mania disabled", () => !selector.ChildrenOfType<BeatmapRulesetTabItem>().Single(t => t.Value.OnlineID == 3).Enabled.Value);

            AddAssert("taiko selected", () => selector.ChildrenOfType<BeatmapRulesetTabItem>().Single(t => t.Active.Value).Value.OnlineID == 1);
        }

        [Test]
        public void TestSingleRulesetBeatmapSet()
        {
            AddStep("load single ruleset beatmapset", () =>
            {
                selector.BeatmapSet = new APIBeatmapSet
                {
                    Beatmaps = new[] { new APIBeatmap { RulesetID = 3 } }
                };
            });

            AddAssert("single ruleset selected", () => selector.ChildrenOfType<BeatmapRulesetTabItem>().Single(t => t.Active.Value).Value.OnlineID == 3);
        }

        [Test]
        public void TestEmptyBeatmapSet()
        {
            AddStep("load empty beatmapset", () => selector.BeatmapSet = new APIBeatmapSet());
            AddAssert("all rulesets disabled", () => selector.ChildrenOfType<BeatmapRulesetTabItem>().All(t => !t.Active.Value && !t.Enabled.Value));
        }

        [Test]
        public void TestNullBeatmapSet()
        {
            AddStep("load null beatmapset", () => selector.BeatmapSet = null);
            AddAssert("all rulesets disabled", () => selector.ChildrenOfType<BeatmapRulesetTabItem>().All(t => !t.Active.Value && !t.Enabled.Value));
        }
    }
}
