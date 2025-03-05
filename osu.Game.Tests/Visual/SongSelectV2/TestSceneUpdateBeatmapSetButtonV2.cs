// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneUpdateBeatmapSetButtonV2 : OsuTestScene
    {
        private UpdateBeatmapSetButton button = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = button = new UpdateBeatmapSetButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        });

        [Test]
        public void TestNullBeatmap()
        {
            AddStep("null beatmap", () => button.BeatmapSet = null);
            AddAssert("button invisible", () => button.Alpha == 0f);
        }

        [Test]
        public void TestUpdatedBeatmap()
        {
            AddStep("updated beatmap", () => button.BeatmapSet = new BeatmapSetInfo
            {
                Beatmaps = { new BeatmapInfo() }
            });
            AddAssert("button invisible", () => button.Alpha == 0f);
        }

        [Test]
        public void TestNonUpdatedBeatmap()
        {
            AddStep("non-updated beatmap", () => button.BeatmapSet = new BeatmapSetInfo
            {
                Beatmaps =
                {
                    new BeatmapInfo
                    {
                        MD5Hash = "test",
                        OnlineMD5Hash = "online",
                        LastOnlineUpdate = DateTimeOffset.Now,
                    }
                }
            });

            AddAssert("button visible", () => button.Alpha == 1f);
        }
    }
}
