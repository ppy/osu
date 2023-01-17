// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Configuration;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Osu;
using osuTK;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public partial class TestSceneBeatmapCardDownloadButton : OsuTestScene
    {
        private DownloadButton downloadButton;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Test]
        public void TestDownloadableBeatmapWithVideo()
        {
            createButton(true, true);
            assertDownloadEnabled(true);

            AddStep("prefer no video", () => config.SetValue(OsuSetting.PreferNoVideo, true));
            AddAssert("tooltip text correct", () => downloadButton.TooltipText == BeatmapsetsStrings.PanelDownloadNoVideo);

            AddStep("prefer video", () => config.SetValue(OsuSetting.PreferNoVideo, false));
            AddAssert("tooltip text correct", () => downloadButton.TooltipText == BeatmapsetsStrings.PanelDownloadVideo);
        }

        [Test]
        public void TestUndownloadableBeatmap()
        {
            createButton(false);
            assertDownloadEnabled(false);
            AddAssert("tooltip text correct", () => downloadButton.TooltipText == BeatmapsetsStrings.AvailabilityDisabled);
        }

        private void assertDownloadEnabled(bool enabled) => AddAssert($"download {(enabled ? "enabled" : "disabled")}", () => downloadButton.Enabled.Value == enabled);

        private void createButton(bool downloadable, bool hasVideo = false)
        {
            AddStep("create button", () =>
            {
                Child = downloadButton = new DownloadButton(downloadable ? getDownloadableBeatmapSet(hasVideo) : getUndownloadableBeatmapSet())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(25f, 50f),
                    Scale = new Vector2(2f),
                    State = { Value = DownloadState.NotDownloaded },
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
    }
}
