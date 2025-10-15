// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.Cursor;
using osu.Game.Scoring;
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
                        Child = new OsuContextMenuContainer
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
                        }
                    };
                });
            }
        }

        [Test]
        public void TestRanks()
        {
            for (int i = -1; i <= 7; i++)
            {
                ScoreRank rank = (ScoreRank)i;

                AddStep($"display rank {rank}", () =>
                {
                    ContentContainer.Child = new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Aquamarine))
                        },
                        Child = new OsuContextMenuContainer
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
                                Children = new[]
                                {
                                    new PanelGroupRankDisplay
                                    {
                                        Item = new CarouselItem(new RankDisplayGroupDefinition(rank))
                                    },
                                    new PanelGroupRankDisplay
                                    {
                                        Item = new CarouselItem(new RankDisplayGroupDefinition(rank)),
                                        KeyboardSelected = { Value = true },
                                    },
                                    new PanelGroupRankDisplay
                                    {
                                        Item = new CarouselItem(new RankDisplayGroupDefinition(rank)),
                                        Expanded = { Value = true },
                                    },
                                    new PanelGroupRankDisplay
                                    {
                                        Item = new CarouselItem(new RankDisplayGroupDefinition(rank)),
                                        Expanded = { Value = true },
                                        KeyboardSelected = { Value = true },
                                    },
                                },
                            }
                        }
                    };
                });
            }
        }

        [Test]
        public void TestStatuses()
        {
            foreach (var status in Enum.GetValues<BeatmapOnlineStatus>().Where(s => s != BeatmapOnlineStatus.Approved))
            {
                AddStep($"display {status} status", () =>
                {
                    ContentContainer.Child = new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Aquamarine))
                        },
                        Child = new OsuContextMenuContainer
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
                                Children = new[]
                                {
                                    new PanelGroupRankedStatus
                                    {
                                        Item = new CarouselItem(new RankedStatusGroupDefinition(0, status))
                                    },
                                    new PanelGroupRankedStatus
                                    {
                                        Item = new CarouselItem(new RankedStatusGroupDefinition(1, status)),
                                        KeyboardSelected = { Value = true },
                                    },
                                    new PanelGroupRankedStatus
                                    {
                                        Item = new CarouselItem(new RankedStatusGroupDefinition(2, status)),
                                        Expanded = { Value = true },
                                    },
                                    new PanelGroupRankedStatus
                                    {
                                        Item = new CarouselItem(new RankedStatusGroupDefinition(3, status)),
                                        Expanded = { Value = true },
                                        KeyboardSelected = { Value = true },
                                    },
                                },
                            }
                        }
                    };
                });
            }
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
                }
            };
        }
    }
}
