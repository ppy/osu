// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.Cursor;
using osu.Game.Overlays;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestScenePanelSet : ThemeComparisonTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private BeatmapSetInfo beatmapSet = null!;

        public TestScenePanelSet()
            : base(false)
        {
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            beatmapSet = beatmaps.GetAllUsableBeatmapSets().FirstOrDefault(b => b.OnlineID == 241526)
                         ?? beatmaps.GetAllUsableBeatmapSets().FirstOrDefault(b => !b.Protected)
                         ?? TestResources.CreateTestBeatmapSetInfo();
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
                beatmapSet = randomSet;

                CreateThemedContent(OverlayColourScheme.Aquamarine);
            });
        }

        protected override Drawable CreateContent()
        {
            return new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
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
                        new PanelBeatmapSet
                        {
                            Item = new CarouselItem(new GroupedBeatmapSet(null, beatmapSet))
                        },
                        new PanelBeatmapSet
                        {
                            Item = new CarouselItem(new GroupedBeatmapSet(null, beatmapSet)),
                            KeyboardSelected = { Value = true }
                        },
                        new PanelBeatmapSet
                        {
                            Item = new CarouselItem(new GroupedBeatmapSet(null, beatmapSet)),
                            Expanded = { Value = true }
                        },
                        new PanelBeatmapSet
                        {
                            Item = new CarouselItem(new GroupedBeatmapSet(null, beatmapSet)),
                            KeyboardSelected = { Value = true },
                            Expanded = { Value = true }
                        },
                    }
                }
            };
        }
    }
}
