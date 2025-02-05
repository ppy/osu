// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapCarouselGroupPanel : ThemeComparisonTestScene
    {
        public TestSceneBeatmapCarouselGroupPanel()
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
                    new GroupPanel
                    {
                        Item = new CarouselItem(new GroupDefinition("Group A"))
                    },
                    new GroupPanel
                    {
                        Item = new CarouselItem(new GroupDefinition("Group A")),
                        KeyboardSelected = { Value = true }
                    },
                    new GroupPanel
                    {
                        Item = new CarouselItem(new GroupDefinition("Group A")),
                        Selected = { Value = true }
                    },
                    new GroupPanel
                    {
                        Item = new CarouselItem(new GroupDefinition("Group A")),
                        KeyboardSelected = { Value = true },
                        Selected = { Value = true }
                    },
                    new StarsGroupPanel
                    {
                        Item = new CarouselItem(new StarsGroupDefinition(1))
                    },
                    new StarsGroupPanel
                    {
                        Item = new CarouselItem(new StarsGroupDefinition(3)),
                    },
                    new StarsGroupPanel
                    {
                        Item = new CarouselItem(new StarsGroupDefinition(5)),
                    },
                    new StarsGroupPanel
                    {
                        Item = new CarouselItem(new StarsGroupDefinition(7)),
                    },
                    new StarsGroupPanel
                    {
                        Item = new CarouselItem(new StarsGroupDefinition(8)),
                    },
                    new StarsGroupPanel
                    {
                        Item = new CarouselItem(new StarsGroupDefinition(9)),
                    },
                }
            };
        }
    }
}
