// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Configuration;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public class TestSceneBeatmapCardDownloadButton : OsuTestScene
    {
        private TestDownloadButton downloadButton;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Test]
        public void TestDownloadableBeatmap()
        {
            ensureSoleilyRemoved();
            createButton(true);

            assertDownloadVisible(true);
            assertDownloadEnabled(true);
            assertPlayVisible(false);
            AddAssert("tooltip text correct", () => downloadButton.Download.TooltipText == BeatmapsetsStrings.PanelDownloadAll);

            AddStep("set downloading state", () => downloadButton.State.Value = DownloadState.Downloading);
            assertDownloadVisible(true);
            assertDownloadEnabled(false);
            assertPlayVisible(false);

            AddStep("set importing state", () => downloadButton.State.Value = DownloadState.Importing);
            assertDownloadVisible(true);
            assertDownloadEnabled(false);
            assertPlayVisible(false);

            AddStep("set locally available state", () => downloadButton.State.Value = DownloadState.LocallyAvailable);
            assertDownloadVisible(false);
            assertPlayVisible(true);
        }

        [Test]
        public void TestDownloadableBeatmapWithVideo()
        {
            createButton(true, true);
            assertDownloadEnabled(true);

            AddStep("prefer no video", () => config.SetValue(OsuSetting.PreferNoVideo, true));
            AddAssert("tooltip text correct", () => downloadButton.Download.TooltipText == BeatmapsetsStrings.PanelDownloadNoVideo);

            AddStep("prefer video", () => config.SetValue(OsuSetting.PreferNoVideo, false));
            AddAssert("tooltip text correct", () => downloadButton.Download.TooltipText == BeatmapsetsStrings.PanelDownloadVideo);
        }

        [Test]
        public void TestUndownloadableBeatmap()
        {
            createButton(false);
            assertDownloadEnabled(false);
            AddAssert("tooltip text correct", () => downloadButton.Download.TooltipText == BeatmapsetsStrings.AvailabilityDisabled);
        }

        [Test]
        public void TestDownloadState()
        {
            ensureSoleilyRemoved();
            createButtonWithBeatmap(createSoleily());
            AddAssert("button state not downloaded", () => downloadButton.State.Value == DownloadState.NotDownloaded);
            AddStep("import soleily", () => beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()));

            AddUntilStep("wait for beatmap import", () => beatmaps.GetAllUsableBeatmapSets().Any(b => b.OnlineID == 241526));
            AddUntilStep("button state downloaded", () => downloadButton.State.Value == DownloadState.LocallyAvailable);

            createButtonWithBeatmap(createSoleily());
            AddUntilStep("button state downloaded", () => downloadButton.State.Value == DownloadState.LocallyAvailable);
            ensureSoleilyRemoved();
            AddUntilStep("button state not downloaded", () => downloadButton.State.Value == DownloadState.NotDownloaded);
        }

        private void ensureSoleilyRemoved()
        {
            AddUntilStep("ensure manager loaded", () => beatmaps != null);
            AddStep("remove soleily", () =>
            {
                var beatmap = beatmaps.QueryBeatmapSet(b => b.OnlineID == 241526);

                if (beatmap != null) beatmaps.Delete(beatmap);
            });
        }

        private void assertDownloadVisible(bool visible) => AddUntilStep($"download {(visible ? "visible" : "not visible")}", () => downloadButton.Download.IsPresent == visible);
        private void assertDownloadEnabled(bool enabled) => AddAssert($"download {(enabled ? "enabled" : "disabled")}", () => downloadButton.Download.Enabled.Value == enabled);

        private void assertPlayVisible(bool visible) => AddUntilStep($"play {(visible ? "visible" : "not visible")}", () => downloadButton.Play.IsPresent == visible);

        private static APIBeatmapSet createSoleily() => new APIBeatmapSet
        {
            OnlineID = 241526,
            Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = false,
                ExternalLink = string.Empty,
            },
        };

        private void createButtonWithBeatmap(APIBeatmapSet beatmap)
        {
            AddStep("create button", () =>
            {
                Child = downloadButton = new TestDownloadButton(beatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(2)
                };
            });
        }

        private void createButton(bool downloadable, bool hasVideo = false)
        {
            AddStep("create button", () =>
            {
                Child = downloadButton = new TestDownloadButton(downloadable ? getDownloadableBeatmapSet(hasVideo) : getUndownloadableBeatmapSet())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(2)
                };
            });
        }

        private APIBeatmapSet getDownloadableBeatmapSet(bool hasVideo)
        {
            var normal = CreateAPIBeatmapSet(new OsuRuleset().RulesetInfo);
            normal.HasVideo = hasVideo;
            normal.HasStoryboard = true;

            return normal;
        }

        private APIBeatmapSet getUndownloadableBeatmapSet()
        {
            var beatmap = CreateAPIBeatmapSet(new OsuRuleset().RulesetInfo);
            beatmap.Artist = "test";
            beatmap.Title = "undownloadable";
            beatmap.AuthorString = "test";

            beatmap.HasVideo = true;
            beatmap.HasStoryboard = true;

            beatmap.Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = true,
                ExternalLink = "https://osu.ppy.sh",
            };

            return beatmap;
        }

        private class TestDownloadButton : DownloadButton
        {
            public readonly Bindable<DownloadState> State = new Bindable<DownloadState>();
            public readonly BindableNumber<double> Progress = new BindableNumber<double>();

            public new BeatmapCardIconButton Download => base.Download;
            public new BeatmapCardIconButton Play => base.Play;

            public TestDownloadButton(APIBeatmapSet beatmapSet)
                : base(beatmapSet)
            {
                Tracker.State.BindTo(State);
                Tracker.Progress.BindTo(Progress);
            }
        }
    }
}
