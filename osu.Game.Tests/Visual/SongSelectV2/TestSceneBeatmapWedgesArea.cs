// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Visual.SongSelect;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapWedgesArea : SongSelectComponentsTestScene
    {
        private BeatmapDetailsArea detailsArea = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new PopoverContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = 650,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 20 },
                        Children = new Drawable[]
                        {
                            detailsArea = new BeatmapDetailsArea
                            {
                                RelativeSizeAxes = Axes.Both,
                                State = { Value = Visibility.Visible },
                            },
                        },
                    }
                }
            };
        });

        [Test]
        public void TestRulesetChange()
        {
            selectBeatmap(Beatmap.Value.Beatmap);

            AddWaitStep("wait for select", 3);

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                var testBeatmap = TestSceneBeatmapInfoWedge.CreateTestBeatmap(rulesetInfo);

                setRuleset(rulesetInfo);
                selectBeatmap(testBeatmap);
            }
        }

        [Test]
        public void TestWedgeVisibility()
        {
            AddStep("hide", () => detailsArea.Hide());
            AddWaitStep("wait for hide", 3);
            AddAssert("check visibility", () => detailsArea.Alpha == 0);
            AddStep("show", () => detailsArea.Show());
            AddWaitStep("wait for show", 1);
            AddAssert("check visibility", () => detailsArea.Alpha > 0);
        }

        [Test]
        public void TestTruncation()
        {
            selectBeatmap(TestSceneBeatmapInfoWedge.CreateLongMetadata());
        }

        [Test]
        public void TestNullBeatmapWithBackground()
        {
            selectBeatmap(null);
        }

        private void setRuleset(RulesetInfo rulesetInfo)
        {
            AddStep("set ruleset", () => Ruleset.Value = rulesetInfo);
        }

        private void selectBeatmap(IBeatmap? b)
        {
            AddStep($"select {b?.Metadata.Title ?? "null"} beatmap", () => Beatmap.Value = b == null ? Beatmap.Default : CreateWorkingBeatmap(b));
        }
    }
}
