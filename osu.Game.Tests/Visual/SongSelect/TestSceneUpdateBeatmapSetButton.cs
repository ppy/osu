// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Tests.Online;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public class TestSceneUpdateBeatmapSetButton : OsuManualInputManagerTestScene
    {
        private BeatmapCarousel carousel = null!;

        private TestSceneOnlinePlayBeatmapAvailabilityTracker.TestBeatmapModelDownloader beatmapDownloader = null!;

        private BeatmapSetInfo testBeatmapSetInfo = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            var importer = parent.Get<BeatmapManager>();

            dependencies.CacheAs<BeatmapModelDownloader>(beatmapDownloader = new TestSceneOnlinePlayBeatmapAvailabilityTracker.TestBeatmapModelDownloader(importer, API));
            return dependencies;
        }

        private UpdateBeatmapSetButton? getUpdateButton() => carousel.ChildrenOfType<UpdateBeatmapSetButton>().SingleOrDefault();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create carousel", () =>
            {
                Child = carousel = new BeatmapCarousel
                {
                    RelativeSizeAxes = Axes.Both,
                    BeatmapSets = new List<BeatmapSetInfo>
                    {
                        (testBeatmapSetInfo = TestResources.CreateTestBeatmapSetInfo()),
                    }
                };
            });

            AddUntilStep("wait for load", () => carousel.BeatmapSetsLoaded);

            AddAssert("update button not visible", () => getUpdateButton() == null);
        }

        [Test]
        public void TestDownloadToCompletion()
        {
            ArchiveDownloadRequest<IBeatmapSetInfo>? downloadRequest = null;

            AddStep("update online hash", () =>
            {
                testBeatmapSetInfo.Beatmaps.First().OnlineMD5Hash = "different hash";
                testBeatmapSetInfo.Beatmaps.First().LastOnlineUpdate = DateTimeOffset.Now;

                carousel.UpdateBeatmapSet(testBeatmapSetInfo);
            });

            AddUntilStep("only one set visible", () => carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().Count() == 1);
            AddUntilStep("update button visible", () => getUpdateButton() != null);

            AddStep("click button", () => getUpdateButton()?.TriggerClick());

            AddUntilStep("wait for download started", () =>
            {
                downloadRequest = beatmapDownloader.GetExistingDownload(testBeatmapSetInfo);
                return downloadRequest != null;
            });

            AddUntilStep("wait for button disabled", () => getUpdateButton()?.Enabled.Value == false);

            AddUntilStep("progress download to completion", () =>
            {
                if (downloadRequest is TestSceneOnlinePlayBeatmapAvailabilityTracker.TestDownloadRequest testRequest)
                {
                    testRequest.SetProgress(testRequest.Progress + 0.1f);

                    if (testRequest.Progress >= 1)
                    {
                        testRequest.TriggerSuccess();

                        // usually this would be done by the import process.
                        testBeatmapSetInfo.Beatmaps.First().MD5Hash = "different hash";
                        testBeatmapSetInfo.Beatmaps.First().LastOnlineUpdate = DateTimeOffset.Now;

                        // usually this would be done by a realm subscription.
                        carousel.UpdateBeatmapSet(testBeatmapSetInfo);
                        return true;
                    }
                }

                return false;
            });
        }

        [Test]
        public void TestDownloadFailed()
        {
            ArchiveDownloadRequest<IBeatmapSetInfo>? downloadRequest = null;

            AddStep("update online hash", () =>
            {
                testBeatmapSetInfo.Beatmaps.First().OnlineMD5Hash = "different hash";
                testBeatmapSetInfo.Beatmaps.First().LastOnlineUpdate = DateTimeOffset.Now;

                carousel.UpdateBeatmapSet(testBeatmapSetInfo);
            });

            AddUntilStep("only one set visible", () => carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().Count() == 1);
            AddUntilStep("update button visible", () => getUpdateButton() != null);

            AddStep("click button", () => getUpdateButton()?.TriggerClick());

            AddUntilStep("wait for download started", () =>
            {
                downloadRequest = beatmapDownloader.GetExistingDownload(testBeatmapSetInfo);
                return downloadRequest != null;
            });

            AddUntilStep("wait for button disabled", () => getUpdateButton()?.Enabled.Value == false);

            AddUntilStep("progress download to failure", () =>
            {
                if (downloadRequest is TestSceneOnlinePlayBeatmapAvailabilityTracker.TestDownloadRequest testRequest)
                {
                    testRequest.SetProgress(testRequest.Progress + 0.1f);

                    if (testRequest.Progress >= 0.5f)
                    {
                        testRequest.TriggerFailure(new Exception());
                        return true;
                    }
                }

                return false;
            });

            AddUntilStep("wait for button enabled", () => getUpdateButton()?.Enabled.Value == true);
        }
    }
}
