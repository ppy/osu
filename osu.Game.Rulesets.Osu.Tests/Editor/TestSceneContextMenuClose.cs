// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneContextMenuClose : TestSceneOsuEditor
    {
        private ContextMenuContainer contextMenuContainer
            => Editor.ChildrenOfType<ContextMenuContainer>().First();


        [Test]
        public void TestDrawnSliderClosesMenu()
        {
            Playfield playfield = null!;

            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to top left of playfield", () =>
            {
                playfield = this.ChildrenOfType<Playfield>().Single();
                var location = (3 * playfield.ScreenSpaceDrawQuad.TopLeft + playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));
            AddStep("right click circle", () => InputManager.Click(MouseButton.Right));
            AddUntilStep("context menu is visible", () => contextMenuContainer.ChildrenOfType<OsuContextMenu>().Single().State == MenuState.Open);
            AddStep("move mouse to bottom right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopLeft + 3 * playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("select slider placement tool", () => InputManager.Key(Key.Number3));
            AddStep("begin placement", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move mouse to top right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopRight + playfield.ScreenSpaceDrawQuad.BottomLeft) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("end placement", () => InputManager.ReleaseButton(MouseButton.Left));
            AddUntilStep("context menu is not visible", () => contextMenuContainer.ChildrenOfType<OsuContextMenu>().Single().State == MenuState.Closed);
        }

        [Test]
        public void TestCircleClosesMenu()
        {
            Playfield playfield = null!;

            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to top left of playfield", () =>
            {
                playfield = this.ChildrenOfType<Playfield>().Single();
                var location = (3 * playfield.ScreenSpaceDrawQuad.TopLeft + playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));
            AddStep("right click circle", () => InputManager.Click(MouseButton.Right));
            AddUntilStep("context menu is visible", () => contextMenuContainer.ChildrenOfType<OsuContextMenu>().Single().State == MenuState.Open);
            AddStep("move mouse to bottom right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopLeft + 3 * playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("context menu is not visible", () => contextMenuContainer.ChildrenOfType<OsuContextMenu>().Single().State == MenuState.Closed);
        }
    }
}
