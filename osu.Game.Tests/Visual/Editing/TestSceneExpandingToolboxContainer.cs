// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneExpandingToolboxContainer : EditorClockTestScene
    {
        private ExpandingToolboxContainer toolbox = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create toolbox", () =>
            {
                toolbox = new ExpandingToolboxContainer(50, 200)
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(200, 500),
                };
                Child = toolbox;
            });
        }

        [Test]
        public void TestOnMouseUpFunctionality()
        {
            AddStep("click on sidebar", () =>
            {
                InputManager.MoveMouseTo(toolbox.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("sidebar remains expanded", () => toolbox.Expanded.Value);

            AddStep("hold and move cursor inside, release", () =>
            {
                InputManager.MoveMouseTo(toolbox.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(toolbox.ScreenSpaceDrawQuad.BottomLeft);
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("sidebar remains expanded", () => toolbox.Expanded.Value);
        }
    }
}
