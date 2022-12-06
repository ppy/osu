// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tests.Beatmaps.IO;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneUpdateableBeatmapBackgroundSprite : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        private BeatmapSetInfo testBeatmap;
        private IAPIProvider api;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osu, IAPIProvider api)
        {
            this.api = api;

            testBeatmap = BeatmapImportHelper.LoadOszIntoOsu(osu).GetResultSafely();
        }

        [Test]
        public void TestNullBeatmap()
        {
            TestUpdateableBeatmapBackgroundSprite background = null;

            AddStep("load null beatmap", () => Child = background = new TestUpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both });
            AddUntilStep("content loaded", () => background.ContentLoaded);
        }

        [Test]
        public void TestLocalBeatmap()
        {
            TestUpdateableBeatmapBackgroundSprite background = null;

            AddStep("load local beatmap", () =>
            {
                Child = background = new TestUpdateableBeatmapBackgroundSprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Beatmap = { Value = testBeatmap.Beatmaps.First() }
                };
            });

            AddUntilStep("wait for load", () => background.ContentLoaded);
        }

        [Test]
        public void TestOnlineBeatmap()
        {
            if (api.IsLoggedIn)
            {
                var req = new GetBeatmapSetRequest(1);
                api.Queue(req);

                AddUntilStep("wait for api response", () => req.Response != null);

                TestUpdateableBeatmapBackgroundSprite background = null;

                AddStep("load online beatmap", () =>
                {
                    Child = background = new TestUpdateableBeatmapBackgroundSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Beatmap = { Value = new APIBeatmap { BeatmapSet = req.Response } }
                    };
                });

                AddUntilStep("wait for load", () => background.ContentLoaded);
            }
            else
                AddStep("online (login first)", () => { });
        }

        [Test]
        public void TestUnloadAndReload()
        {
            var backgrounds = new List<TestUpdateableBeatmapBackgroundSprite>();
            OsuScrollContainer scrollContainer = null;

            AddStep("create backgrounds hierarchy", () =>
            {
                FillFlowContainer backgroundFlow;

                Child = scrollContainer = new OsuScrollContainer
                {
                    Size = new Vector2(500),
                    Child = backgroundFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10),
                        Padding = new MarginPadding { Bottom = 550 }
                    }
                };

                for (int i = 0; i < 25; i++)
                {
                    var background = new TestUpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both };

                    if (i % 2 == 0)
                        background.Beatmap.Value = testBeatmap.Beatmaps.First();

                    backgroundFlow.Add(new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 100,
                        Masking = true,
                        Child = background
                    });

                    backgrounds.Add(background);
                }
            });

            var loadedBackgrounds = backgrounds.Where(b => b.ContentLoaded);

            AddUntilStep("some loaded", () => loadedBackgrounds.Any());
            AddStep("scroll to bottom", () => scrollContainer.ScrollToEnd());
            AddUntilStep("all unloaded", () => !loadedBackgrounds.Any());
        }

        private partial class TestUpdateableBeatmapBackgroundSprite : UpdateableBeatmapBackgroundSprite
        {
            protected override double UnloadDelay => 2000;

            public bool ContentLoaded => ((DelayedLoadUnloadWrapper)InternalChildren.LastOrDefault())?.Content?.IsLoaded ?? false;
        }
    }
}
