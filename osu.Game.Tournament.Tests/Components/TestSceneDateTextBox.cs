// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneDateTextBox : OsuManualInputManagerTestScene
    {
        private DateTextBox textBox = null!;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = textBox = new DateTextBox
            {
                Width = 0.3f
            };
        });

        [Test]
        public void TestCommitWithoutSettingBindable()
        {
            AddStep("click textbox", () =>
            {
                InputManager.MoveMouseTo(textBox);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("unfocus", () =>
            {
                InputManager.MoveMouseTo(Vector2.Zero);
                InputManager.Click(MouseButton.Left);
            });
        }
    }
}
