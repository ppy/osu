// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestScenePanelGroup : ThemeComparisonTestScene
    {
        public TestScenePanelGroup()
            : base(false)
        {
        }

        [Test]
        public void TestGeneral()
        {
            AddStep("general", () => CreateThemedContent(OverlayColourScheme.Aquamarine));
        }

        [Test]
        public void TestStars()
        {
            for (int i = 0; i <= 10; i++)
            {
                int star = i;

                AddStep($"display {i} star(s)", () =>
                {
                    ContentContainer.Child = new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Aquamarine))
                        },
                        Child = new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Width = 0.5f,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 5f),
                            Children = new[]
                            {
                                new PanelGroupStarDifficulty
                                {
                                    Item = new CarouselItem(new StarDifficultyGroupDefinition(0, $"{star} Star(s)", new StarDifficulty(star, 0)))
                                },
                                new PanelGroupStarDifficulty
                                {
                                    Item = new CarouselItem(new StarDifficultyGroupDefinition(1, $"{star} Star(s)", new StarDifficulty(star, 0))),
                                    KeyboardSelected = { Value = true },
                                },
                                new PanelGroupStarDifficulty
                                {
                                    Item = new CarouselItem(new StarDifficultyGroupDefinition(2, $"{star} Star(s)", new StarDifficulty(star, 0))),
                                    Expanded = { Value = true },
                                },
                                new PanelGroupStarDifficulty
                                {
                                    Item = new CarouselItem(new StarDifficultyGroupDefinition(3, $"{star} Star(s)", new StarDifficulty(star, 0))),
                                    Expanded = { Value = true },
                                    KeyboardSelected = { Value = true },
                                },
                            },
                        }
                    };
                });
            }
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
                }
            };
        }
    }
}
