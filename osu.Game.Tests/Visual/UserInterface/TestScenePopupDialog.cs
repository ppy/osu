// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Overlays.Dialog;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestScenePopupDialog : OsuManualInputManagerTestScene
    {
        private TestPopupDialog dialog = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("new popup", () =>
            {
                Child = dialog = new TestPopupDialog
                {
                    State = { Value = Framework.Graphics.Containers.Visibility.Visible },
                };
            });
        }

        [Test]
        public void TestDangerousButton([Values(false, true)] bool atEdge)
        {
            AddStep("finish transforms", () => dialog.FinishTransforms(true));

            if (atEdge)
            {
                AddStep("move mouse to button edge", () =>
                {
                    var dangerousButtonQuad = dialog.DangerousButton.ScreenSpaceDrawQuad;
                    InputManager.MoveMouseTo(new Vector2(dangerousButtonQuad.TopLeft.X + 5, dangerousButtonQuad.Centre.Y));
                });
            }
            else
                AddStep("move mouse to button", () => InputManager.MoveMouseTo(dialog.DangerousButton));

            AddStep("click button", () => InputManager.Click(MouseButton.Left));
            AddAssert("action not invoked", () => !dialog.DangerousButtonInvoked);

            AddStep("hold button", () => InputManager.PressButton(MouseButton.Left));
            AddUntilStep("action invoked", () => dialog.DangerousButtonInvoked);
            AddStep("release button", () => InputManager.ReleaseButton(MouseButton.Left));
        }

        private partial class TestPopupDialog : PopupDialog
        {
            public PopupDialogDangerousButton DangerousButton { get; }

            public bool DangerousButtonInvoked;

            public TestPopupDialog()
            {
                Icon = FontAwesome.Solid.AssistiveListeningSystems;

                HeaderText = @"This is a test popup";
                BodyText = "I can say lots of stuff and even wrap my words!";

                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogCancelButton
                    {
                        Text = @"Yes. That you can.",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"You're a fake!",
                    },
                    DangerousButton = new PopupDialogDangerousButton
                    {
                        Text = @"Careful with this one..",
                        Action = () => DangerousButtonInvoked = true,
                    },
                };
            }
        }
    }
}
