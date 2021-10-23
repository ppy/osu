// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public class TestSceneBeatmapCardThumbnail : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Test]
        public void TestThumbnailPreview()
        {
            BeatmapCardThumbnail thumbnail = null;

            AddStep("create thumbnail", () => Child = thumbnail = new BeatmapCardThumbnail(CreateAPIBeatmapSet(Ruleset.Value))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200)
            });
            AddToggleStep("toggle dim", dimmed => thumbnail.Dimmed.Value = dimmed);
        }
    }
}
