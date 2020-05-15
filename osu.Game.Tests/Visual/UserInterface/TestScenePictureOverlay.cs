// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestScenePictureOverlay : OsuTestScene
    {
        public TestScenePictureOverlay()
        {
            OnlinePictureOverlay overlay;

            Add(overlay = new OnlinePictureOverlay());

            AddStep("Show Empty", () => overlay.UpdateImage("localhost", true));
            AddStep("Show MATRIX-feather", () => overlay.UpdateImage("https://a.ppy.sh/13870362", true));
            AddStep("show a wide picture", () => overlay.UpdateImage("https://raw.githubusercontent.com/MATRIX-feather/MATRIX-feather.github.io/master/img/endline.png", true));
            AddStep("show a tall picture", () => overlay.UpdateImage("https://raw.githubusercontent.com/MATRIX-feather/MATRIX-feather.github.io/master/img/test.jpg", true));
        }
    }
}
