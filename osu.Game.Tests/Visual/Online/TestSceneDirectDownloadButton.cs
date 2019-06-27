// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osuTK;
using DownloadButton = osu.Game.Overlays.Direct.DownloadButton;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneDirectDownloadButton : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DownloadButton)
        };

        private TestDownloadButton downloadButton;

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

        private void assertEnabled(bool enabled)
        {
            AddAssert($"button {(enabled ? "enabled" : "disabled")}", () => downloadButton.DownloadAllowed == enabled);
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

        private class TestDownloadButton : DownloadButton
        {
            public new bool DownloadAllowed => base.DownloadAllowed;

            public TestDownloadButton(BeatmapSetInfo beatmapSet, bool noVideo = false)
                : base(beatmapSet, noVideo)
            {
            }
        }
    }
}
