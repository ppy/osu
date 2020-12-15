// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneDirectDownloadButton : OsuTestScene
    {
        private TestDownloadButton downloadButton;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Test]
        public void TestDownloadableBeatmap()
        {
            createButton(true);
            assertEnabled(true);
        }

        [Test]
        public void TestUndownloadableBeatmap()
        {
            createButton(false);
            assertEnabled(false);
        }

        [Test]
        public void TestDownloadState()
        {
            AddUntilStep("ensure manager loaded", () => beatmaps != null);
            ensureSoleilyRemoved();
            createButtonWithBeatmap(createSoleily());
            AddAssert("button state not downloaded", () => downloadButton.DownloadState == DownloadState.NotDownloaded);
            AddStep("import soleily", () => beatmaps.Import(TestResources.GetTestBeatmapForImport()));
            AddUntilStep("wait for beatmap import", () => beatmaps.GetAllUsableBeatmapSets().Any(b => b.OnlineBeatmapSetID == 241526));
            createButtonWithBeatmap(createSoleily());
            AddAssert("button state downloaded", () => downloadButton.DownloadState == DownloadState.LocallyAvailable);
            ensureSoleilyRemoved();
            AddAssert("button state not downloaded", () => downloadButton.DownloadState == DownloadState.NotDownloaded);
        }

        private void ensureSoleilyRemoved()
        {
            AddStep("remove soleily", () =>
            {
                var beatmap = beatmaps.QueryBeatmapSet(b => b.OnlineBeatmapSetID == 241526);

                if (beatmap != null) beatmaps.Delete(beatmap);
            });
        }

        private void assertEnabled(bool enabled)
        {
            AddAssert($"button {(enabled ? "enabled" : "disabled")}", () => downloadButton.DownloadEnabled == enabled);
        }

        private BeatmapSetInfo createSoleily()
        {
            return new BeatmapSetInfo
            {
                ID = 1,
                OnlineBeatmapSetID = 241526,
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Availability = new BeatmapSetOnlineAvailability
                    {
                        DownloadDisabled = false,
                        ExternalLink = string.Empty,
                    },
                },
            };
        }

        private void createButtonWithBeatmap(BeatmapSetInfo beatmap)
        {
            AddStep("create button", () =>
            {
                Child = downloadButton = new TestDownloadButton(beatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(75, 50),
                };
            });
        }

        private void createButton(bool downloadable)
        {
            AddStep("create button", () =>
            {
                Child = downloadButton = new TestDownloadButton(downloadable ? getDownloadableBeatmapSet() : getUndownloadableBeatmapSet())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(75, 50),
                };
            });
        }

        private BeatmapSetInfo getDownloadableBeatmapSet()
        {
            var normal = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo).BeatmapSetInfo;
            normal.OnlineInfo.HasVideo = true;
            normal.OnlineInfo.HasStoryboard = true;

            return normal;
        }

        private BeatmapSetInfo getUndownloadableBeatmapSet()
        {
            var beatmap = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo).BeatmapSetInfo;
            beatmap.Metadata.Artist = "test";
            beatmap.Metadata.Title = "undownloadable";
            beatmap.Metadata.AuthorString = "test";

            beatmap.OnlineInfo.HasVideo = true;
            beatmap.OnlineInfo.HasStoryboard = true;

            beatmap.OnlineInfo.Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = true,
                ExternalLink = "http://osu.ppy.sh",
            };

            return beatmap;
        }

        private class TestDownloadButton : BeatmapPanelDownloadButton
        {
            public new bool DownloadEnabled => base.DownloadEnabled;

            public DownloadState DownloadState => State.Value;

            public TestDownloadButton(BeatmapSetInfo beatmapSet)
                : base(beatmapSet)
            {
            }
        }
    }
}
