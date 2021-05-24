// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneUpdateableBeatmapSetCover : OsuTestScene
    {
        [Test]
        public void TestLocal([Values] BeatmapSetCoverType coverType)
        {
            AddStep("setup cover", () => Child = new UpdateableBeatmapSetCover(coverType)
            {
                BeatmapSet = CreateBeatmap(Ruleset.Value).BeatmapInfo.BeatmapSet,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
            });

            AddUntilStep("wait for load", () => this.ChildrenOfType<BeatmapSetCover>().SingleOrDefault()?.IsLoaded ?? false);
        }

        [Test]
        public void TestUnloadAndReload()
        {
            OsuScrollContainer scroll = null;
            List<UpdateableBeatmapSetCover> covers = new List<UpdateableBeatmapSetCover>();

            AddStep("setup covers", () =>
            {
                BeatmapSetInfo setInfo = CreateBeatmap(Ruleset.Value).BeatmapInfo.BeatmapSet;

                FillFlowContainer fillFlow;

                Child = scroll = new OsuScrollContainer
                {
                    Size = new Vector2(500f),
                    Child = fillFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10),
                        Padding = new MarginPadding { Bottom = 550 }
                    }
                };

                var coverTypes = Enum.GetValues(typeof(BeatmapSetCoverType))
                                     .Cast<BeatmapSetCoverType>()
                                     .ToList();

                for (int i = 0; i < 25; i++)
                {
                    var coverType = coverTypes[i % coverTypes.Count];

                    var cover = new UpdateableBeatmapSetCover(coverType)
                    {
                        BeatmapSet = setInfo,
                        Height = 100,
                        Masking = true,
                    };

                    if (coverType == BeatmapSetCoverType.Cover)
                        cover.Width = 500;
                    else if (coverType == BeatmapSetCoverType.Card)
                        cover.Width = 400;
                    else if (coverType == BeatmapSetCoverType.List)
                        cover.Size = new Vector2(100, 50);

                    fillFlow.Add(cover);
                    covers.Add(cover);
                }
            });

            var loadedCovers = covers.Where(c => c.ChildrenOfType<BeatmapSetCover>().SingleOrDefault()?.IsLoaded ?? false);

            AddUntilStep("some loaded", () => loadedCovers.Any());
            AddStep("scroll to end", () => scroll.ScrollToEnd());
            AddUntilStep("all unloaded", () => !loadedCovers.Any());
        }

        [Test]
        public void TestSetNullBeatmapWhileLoading()
        {
            TestUpdateableBeatmapSetCover updateableCover = null;

            AddStep("setup cover", () => Child = updateableCover = new TestUpdateableBeatmapSetCover
            {
                BeatmapSet = CreateBeatmap(Ruleset.Value).BeatmapInfo.BeatmapSet,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
            });

            AddStep("change model", () => updateableCover.BeatmapSet = null);
            AddWaitStep("wait some", 5);
            AddAssert("no cover added", () => !updateableCover.ChildrenOfType<DelayedLoadUnloadWrapper>().Any());
        }

        [Test]
        public void TestCoverChangeOnNewBeatmap()
        {
            TestUpdateableBeatmapSetCover updateableCover = null;
            BeatmapSetCover initialCover = null;

            AddStep("setup cover", () => Child = updateableCover = new TestUpdateableBeatmapSetCover(0)
            {
                BeatmapSet = createBeatmapWithCover("https://assets.ppy.sh/beatmaps/1189904/covers/cover.jpg"),
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Alpha = 0.4f
            });

            AddUntilStep("cover loaded", () => updateableCover.ChildrenOfType<BeatmapSetCover>().Any());
            AddStep("store initial cover", () => initialCover = updateableCover.ChildrenOfType<BeatmapSetCover>().Single());
            AddUntilStep("wait for fade complete", () => initialCover.Alpha == 1);

            AddStep("switch beatmap",
                () => updateableCover.BeatmapSet = createBeatmapWithCover("https://assets.ppy.sh/beatmaps/1079428/covers/cover.jpg"));
            AddUntilStep("new cover loaded", () => updateableCover.ChildrenOfType<BeatmapSetCover>().Except(new[] { initialCover }).Any());
        }

        private static BeatmapSetInfo createBeatmapWithCover(string coverUrl) => new BeatmapSetInfo
        {
            OnlineInfo = new BeatmapSetOnlineInfo
            {
                Covers = new BeatmapSetOnlineCovers { Cover = coverUrl }
            }
        };

        private class TestUpdateableBeatmapSetCover : UpdateableBeatmapSetCover
        {
            private readonly int loadDelay;

            public TestUpdateableBeatmapSetCover(int loadDelay = 10000)
            {
                this.loadDelay = loadDelay;
            }

            protected override Drawable CreateDrawable(BeatmapSetInfo model)
            {
                if (model == null)
                    return null;

                return new TestBeatmapSetCover(model, loadDelay)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                };
            }
        }

        private class TestBeatmapSetCover : BeatmapSetCover
        {
            private readonly int loadDelay;

            public TestBeatmapSetCover(BeatmapSetInfo set, int loadDelay)
                : base(set)
            {
                this.loadDelay = loadDelay;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Thread.Sleep(loadDelay);
            }
        }
    }
}
