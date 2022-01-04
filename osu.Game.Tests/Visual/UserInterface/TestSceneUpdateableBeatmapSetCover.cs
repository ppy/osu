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
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneUpdateableBeatmapSetCover : OsuTestScene
    {
        [Test]
        public void TestLocal([Values] BeatmapSetCoverType coverType)
        {
            AddStep("setup cover", () => Child = new UpdateableOnlineBeatmapSetCover(coverType)
            {
                OnlineInfo = CreateAPIBeatmapSet(),
                RelativeSizeAxes = Axes.Both,
                Masking = true,
            });

            AddUntilStep("wait for load", () => this.ChildrenOfType<OnlineBeatmapSetCover>().SingleOrDefault()?.IsLoaded ?? false);
        }

        [Test]
        public void TestUnloadAndReload()
        {
            OsuScrollContainer scroll = null;
            List<UpdateableOnlineBeatmapSetCover> covers = new List<UpdateableOnlineBeatmapSetCover>();

            AddStep("setup covers", () =>
            {
                var beatmapSet = CreateAPIBeatmapSet();

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

                    var cover = new UpdateableOnlineBeatmapSetCover(coverType)
                    {
                        OnlineInfo = beatmapSet,
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

            var loadedCovers = covers.Where(c => c.ChildrenOfType<OnlineBeatmapSetCover>().SingleOrDefault()?.IsLoaded ?? false);

            AddUntilStep("some loaded", () => loadedCovers.Any());
            AddStep("scroll to end", () => scroll.ScrollToEnd());
            AddUntilStep("all unloaded", () => !loadedCovers.Any());
        }

        [Test]
        public void TestSetNullBeatmapWhileLoading()
        {
            TestUpdateableOnlineBeatmapSetCover updateableCover = null;

            AddStep("setup cover", () => Child = updateableCover = new TestUpdateableOnlineBeatmapSetCover
            {
                OnlineInfo = CreateAPIBeatmapSet(),
                RelativeSizeAxes = Axes.Both,
                Masking = true,
            });

            AddStep("change model", () => updateableCover.OnlineInfo = null);
            AddWaitStep("wait some", 5);
            AddAssert("no cover added", () => !updateableCover.ChildrenOfType<DelayedLoadUnloadWrapper>().Any());
        }

        [Test]
        public void TestCoverChangeOnNewBeatmap()
        {
            TestUpdateableOnlineBeatmapSetCover updateableCover = null;
            OnlineBeatmapSetCover initialCover = null;

            AddStep("setup cover", () => Child = updateableCover = new TestUpdateableOnlineBeatmapSetCover(0)
            {
                OnlineInfo = createBeatmapWithCover("https://assets.ppy.sh/beatmaps/1189904/covers/cover.jpg"),
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Alpha = 0.4f
            });

            AddUntilStep("cover loaded", () => updateableCover.ChildrenOfType<OnlineBeatmapSetCover>().Any());
            AddStep("store initial cover", () => initialCover = updateableCover.ChildrenOfType<OnlineBeatmapSetCover>().Single());
            AddUntilStep("wait for fade complete", () => initialCover.Alpha == 1);

            AddStep("switch beatmap",
                () => updateableCover.OnlineInfo = createBeatmapWithCover("https://assets.ppy.sh/beatmaps/1079428/covers/cover.jpg"));
            AddUntilStep("new cover loaded", () => updateableCover.ChildrenOfType<OnlineBeatmapSetCover>().Except(new[] { initialCover }).Any());
        }

        private static APIBeatmapSet createBeatmapWithCover(string coverUrl) => new APIBeatmapSet
        {
            Covers = new BeatmapSetOnlineCovers { Cover = coverUrl }
        };

        private class TestUpdateableOnlineBeatmapSetCover : UpdateableOnlineBeatmapSetCover
        {
            private readonly int loadDelay;

            public TestUpdateableOnlineBeatmapSetCover(int loadDelay = 10000)
            {
                this.loadDelay = loadDelay;
            }

            protected override Drawable CreateDrawable(IBeatmapSetOnlineInfo model)
            {
                if (model == null)
                    return null;

                return new TestOnlineBeatmapSetCover(model, loadDelay)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                };
            }
        }

        private class TestOnlineBeatmapSetCover : OnlineBeatmapSetCover
        {
            private readonly int loadDelay;

            public TestOnlineBeatmapSetCover(IBeatmapSetOnlineInfo set, int loadDelay)
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
