// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneExpandingToolboxContainer : EditorClockTestScene
    {
        private ExpandingToolboxContainer toolbox = null!;
        private Bindable<bool> contractSidebars = null!;

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
            AddStep("load contractSidebars configuration", () =>
            {
                var config = new OsuConfigManager(LocalStorage);
                contractSidebars = config.GetBindable<bool>(OsuSetting.EditorContractSidebars);
                contractSidebars.Value = true;
            });
        }

        [Test]
        public void TestExpandingToolbox()
        {
            AddStep("state - sidebar collapsed", () => toolbox.Expanded.Value = false);
            AddStep("click on toolbox", () =>
            {
                InputManager.MoveMouseTo(toolbox.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("sidebar expands after click", () => toolbox.Expanded.Value);

            AddStep("state - sidebar collapsed", () => toolbox.Expanded.Value = false);

            AddStep("hold and move cursor inside", () =>
            {
                InputManager.MoveMouseTo(toolbox.ScreenSpaceDrawQuad.TopLeft);
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(toolbox.ScreenSpaceDrawQuad.BottomRight);
                InputManager.ReleaseButton(MouseButton.Left);
                AddAssert("sidebar remains collapsed", () => !toolbox.Expanded.Value);
            });
        }
    }
}
