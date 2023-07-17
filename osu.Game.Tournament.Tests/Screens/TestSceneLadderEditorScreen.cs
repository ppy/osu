// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Screens.Editors;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneLadderEditorScreen : TournamentTestScene
    {
        private LadderEditorScreen ladderEditorScreen;
        private OsuContextMenuContainer osuContextMenuContainer;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Add(osuContextMenuContainer = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = ladderEditorScreen = new LadderEditorScreen()
            });
        });

        [Test]
        public void TestResetBracketTeams()
        {
            AddStep("pull up context menu", () =>
            {
                InputManager.MoveMouseTo(ladderEditorScreen);
                InputManager.Click(MouseButton.Right);
            });

            AddStep("click Reset teams button", () =>
            {
                InputManager.MoveMouseTo(osuContextMenuContainer.ChildrenOfType<DrawableOsuMenuItem>().Last(p =>
                    ((OsuMenuItem)p.Item).Type == MenuItemType.Destructive), new Vector2(5, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("dialog displayed", () => dialogOverlay.CurrentDialog is LadderResetTeamsDialog);

            AddStep("click confirmation", () =>
            {
                InputManager.MoveMouseTo(dialogOverlay.CurrentDialog.ChildrenOfType<PopupDialogButton>().First());
                InputManager.PressButton(MouseButton.Left);
            });

            AddUntilStep("dialog dismissed", () => dialogOverlay.CurrentDialog is not LadderResetTeamsDialog);

            AddStep("release mouse button", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("assert ladder teams reset", () =>
            {
                return Ladder.Matches.All(m => m.Team1.Value == null && m.Team2.Value == null);
            });
        }

        [Test]
        public void TestResetBracketTeamsCancelled()
        {
            AddStep("pull up context menu", () =>
            {
                InputManager.MoveMouseTo(ladderEditorScreen);
                InputManager.Click(MouseButton.Right);
            });

            AddStep("click Reset teams button", () =>
            {
                InputManager.MoveMouseTo(osuContextMenuContainer.ChildrenOfType<DrawableOsuMenuItem>().Last(p =>
                    ((OsuMenuItem)p.Item).Type == MenuItemType.Destructive), new Vector2(5, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("dialog displayed", () => dialogOverlay.CurrentDialog is LadderResetTeamsDialog);
            AddStep("click cancel", () =>
            {
                InputManager.MoveMouseTo(dialogOverlay.CurrentDialog.ChildrenOfType<PopupDialogButton>().Last());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("dialog dismissed", () => dialogOverlay.CurrentDialog is not LadderResetTeamsDialog);

            AddAssert("assert ladder teams unchanged", () =>
            {
                return !Ladder.Matches.Any(m => m.Team1.Value == null && m.Team2.Value == null);
            });
        }
    }
}
