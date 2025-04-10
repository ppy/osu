// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapCarouselV2DifficultyPanel : ThemeComparisonTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private BeatmapInfo beatmap = null!;

        public TestSceneBeatmapCarouselV2DifficultyPanel()
            : base(false)
        {
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            var beatmapSet = beatmaps.GetAllUsableBeatmapSets().FirstOrDefault(b => b.OnlineID == 241526)
                             ?? beatmaps.GetAllUsableBeatmapSets().FirstOrDefault(b => !b.Protected)
                             ?? TestResources.CreateTestBeatmapSetInfo();

            beatmap = beatmapSet.Beatmaps.First();
        });

        [Test]
        public void TestDisplay()
        {
            AddStep("display", () => CreateThemedContent(OverlayColourScheme.Aquamarine));
        }

        [Test]
        public void TestRandomBeatmap()
        {
            AddStep("random beatmap", () =>
            {
                var randomSet = beatmaps.GetAllUsableBeatmapSets().MinBy(_ => RNG.Next());
                randomSet ??= TestResources.CreateTestBeatmapSetInfo();
                beatmap = randomSet.Beatmaps.MinBy(_ => RNG.Next())!;

                CreateThemedContent(OverlayColourScheme.Aquamarine);
            });
        }

        [Test]
        public void TestManiaRuleset()
        {
            AddToggleStep("mania ruleset", v => Ruleset.Value = v ? new ManiaRuleset().RulesetInfo : new OsuRuleset().RulesetInfo);
        }

        protected override Drawable CreateContent()
        {
            return new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 5f),
                Children = new Drawable[]
                {
                    new PanelBeatmap
                    {
                        Item = new CarouselItem(beatmap)
                    },
                    new PanelBeatmap
                    {
                        Item = new CarouselItem(beatmap),
                        KeyboardSelected = { Value = true }
                    },
                    new PanelBeatmap
                    {
                        Item = new CarouselItem(beatmap),
                        Selected = { Value = true }
                    },
                    new PanelBeatmap
                    {
                        Item = new CarouselItem(beatmap),
                        KeyboardSelected = { Value = true },
                        Selected = { Value = true }
                    },
                }
            };
        }
    }
}
