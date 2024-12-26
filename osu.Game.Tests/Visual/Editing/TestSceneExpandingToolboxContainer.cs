// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Edit;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneExpandingToolboxContainer : OsuManualInputManagerTestScene
    {
        private ExpandingToolboxContainer toolbox = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create toolbox", () =>
            {
                toolbox = new ExpandingToolboxContainer(50, 200)
                {
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1000,
                        Colour = Colour4.Red,
                    }
                };
                Child = toolbox;
            });
        }

        [Test]
        public void TestExpandingToolbox()
        {
            AddStep("collapse toolbox", () => toolbox.Expanded.Value = false);
            AddStep("click on toolbox", () =>
            {
                InputManager.MoveMouseTo(toolbox.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("toolbox expanded", () => toolbox.Expanded.Value);

            AddStep("collapse toolbox", () => toolbox.Expanded.Value = false);
            AddStep("drag cursor inside", () =>
            {
                InputManager.MoveMouseTo(toolbox.ScreenSpaceDrawQuad.TopLeft);
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(toolbox.ScreenSpaceDrawQuad.BottomRight);
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("toolbox remains collapsed", () => !toolbox.Expanded.Value);
        }
    }
}
