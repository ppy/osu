// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapCarouselV2GroupPanel : ThemeComparisonTestScene
    {
        public TestSceneBeatmapCarouselV2GroupPanel()
            : base(false)
        {
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
                    new PanelGroup
                    {
                        Item = new CarouselItem(new GroupDefinition('A', "Group A"))
                    },
                    new PanelGroup
                    {
                        Item = new CarouselItem(new GroupDefinition('A', "Group A")),
                        KeyboardSelected = { Value = true }
                    },
                    new PanelGroup
                    {
                        Item = new CarouselItem(new GroupDefinition('A', "Group A")),
                        Expanded = { Value = true }
                    },
                    new PanelGroup
                    {
                        Item = new CarouselItem(new GroupDefinition('A', "Group A")),
                        KeyboardSelected = { Value = true },
                        Expanded = { Value = true }
                    },
                    new PanelGroupStarDifficulty
                    {
                        Item = new CarouselItem(new GroupDefinition(1, "1"))
                    },
                    new PanelGroupStarDifficulty
                    {
                        Item = new CarouselItem(new GroupDefinition(3, "3")),
                        Expanded = { Value = true }
                    },
                    new PanelGroupStarDifficulty
                    {
                        Item = new CarouselItem(new GroupDefinition(5, "5")),
                    },
                    new PanelGroupStarDifficulty
                    {
                        Item = new CarouselItem(new GroupDefinition(7, "7")),
                        Expanded = { Value = true }
                    },
                    new PanelGroupStarDifficulty
                    {
                        Item = new CarouselItem(new GroupDefinition(8, "8")),
                    },
                    new PanelGroupStarDifficulty
                    {
                        Item = new CarouselItem(new GroupDefinition(9, "9")),
                        Expanded = { Value = true }
                    },
                }
            };
        }
    }
}
