// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public class TestSceneBeatmapCardThumbnail : OsuManualInputManagerTestScene
    {
        private PlayButton playButton => this.ChildrenOfType<PlayButton>().Single();

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Test]
        public void TestThumbnailPreview()
        {
            BeatmapCardThumbnail thumbnail = null;

            AddStep("create thumbnail", () =>
            {
                var beatmapSet = CreateAPIBeatmapSet(Ruleset.Value);
                beatmapSet.OnlineID = 241526; // ID hardcoded to ensure that the preview track exists online.

                Child = thumbnail = new BeatmapCardThumbnail(beatmapSet)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(200)
                };
            });
            AddStep("enable dim", () => thumbnail.Dimmed.Value = true);
            AddUntilStep("button visible", () => playButton.IsPresent);

            AddStep("click button", () =>
            {
                InputManager.MoveMouseTo(playButton);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("wait for start", () => playButton.Playing.Value && playButton.Enabled.Value);
            iconIs(FontAwesome.Solid.Stop);

            AddStep("click again", () =>
            {
                InputManager.MoveMouseTo(playButton);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("wait for stop", () => !playButton.Playing.Value && playButton.Enabled.Value);
            iconIs(FontAwesome.Solid.Play);

            AddStep("click again", () =>
            {
                InputManager.MoveMouseTo(playButton);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("wait for start", () => playButton.Playing.Value && playButton.Enabled.Value);
            iconIs(FontAwesome.Solid.Stop);

            AddStep("disable dim", () => thumbnail.Dimmed.Value = false);
            AddWaitStep("wait some", 3);
            AddAssert("button still visible", () => playButton.IsPresent);

            // The track plays in real-time, so we need to check for progress in increments to avoid timeout.
            AddUntilStep("progress > 0.25", () => thumbnail.ChildrenOfType<PlayButton>().Single().Progress.Value > 0.25);
            AddUntilStep("progress > 0.5", () => thumbnail.ChildrenOfType<PlayButton>().Single().Progress.Value > 0.5);
            AddUntilStep("progress > 0.75", () => thumbnail.ChildrenOfType<PlayButton>().Single().Progress.Value > 0.75);

            AddUntilStep("wait for track to end", () => !playButton.Playing.Value);
            AddUntilStep("button hidden", () => !playButton.IsPresent);
        }

        private void iconIs(IconUsage usage) => AddUntilStep("icon is correct", () => playButton.ChildrenOfType<SpriteIcon>().Any(icon => icon.Icon.Equals(usage)));
    }
}
