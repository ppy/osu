// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Timing;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneTapButton : OsuManualInputManagerTestScene
    {
        private TapButton tapButton;

        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Test]
        public void TestBasic()
        {
            AddStep("create button", () =>
            {
                Child = tapButton = new TapButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(4),
                };
            });

            bool pressed = false;

            AddRepeatStep("Press button", () =>
            {
                InputManager.MoveMouseTo(tapButton);
                if (!pressed)
                    InputManager.PressButton(MouseButton.Left);
                else
                    InputManager.ReleaseButton(MouseButton.Left);

                pressed = !pressed;
            }, 100);
        }
    }
}
