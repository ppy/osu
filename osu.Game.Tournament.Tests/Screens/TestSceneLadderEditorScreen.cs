// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Screens.Editors;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Screens.Editors.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneLadderEditorScreen : TournamentScreenTestScene
    {
        private LadderEditorScreen ladderEditorScreen = null!;
        private OsuContextMenuContainer? osuContextMenuContainer;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            ladderEditorScreen = new LadderEditorScreen();
            Add(osuContextMenuContainer = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = ladderEditorScreen = new LadderEditorScreen()
            });
        });

        [Test]
        public void TestResetBracketTeamsCancelled()
        {
            Bindable<string> matchBeforeReset = new Bindable<string>();
            AddStep("save current match state", () =>
            {
                matchBeforeReset.Value = JsonConvert.SerializeObject(Ladder.CurrentMatch.Value);
            });
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

            AddAssert("dialog displayed", () => DialogOverlay.CurrentDialog is LadderResetTeamsDialog);
            AddStep("click cancel", () =>
            {
                InputManager.MoveMouseTo(DialogOverlay.CurrentDialog.ChildrenOfType<PopupDialogButton>().Last());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("dialog dismissed", () => DialogOverlay.CurrentDialog is not LadderResetTeamsDialog);

            AddAssert("assert ladder teams unchanged", () => string.Equals(matchBeforeReset.Value, JsonConvert.SerializeObject(Ladder.CurrentMatch.Value), StringComparison.Ordinal));
        }

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

            AddAssert("dialog displayed", () => DialogOverlay.CurrentDialog is LadderResetTeamsDialog);

            AddStep("click confirmation", () =>
            {
                InputManager.MoveMouseTo(DialogOverlay.CurrentDialog.ChildrenOfType<PopupDialogButton>().First());
                InputManager.PressButton(MouseButton.Left);
            });

            AddUntilStep("dialog dismissed", () => DialogOverlay.CurrentDialog is not LadderResetTeamsDialog);

            AddStep("release mouse button", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("assert ladder teams reset", () => Ladder.CurrentMatch.Value?.Team1.Value == null && Ladder.CurrentMatch.Value?.Team2.Value == null);
        }
    }
}
