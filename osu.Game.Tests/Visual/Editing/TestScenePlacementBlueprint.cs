// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestScenePlacementBlueprint : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        private GlobalActionContainer globalActionContainer => this.ChildrenOfType<GlobalActionContainer>().Single();

        [Test]
        public void TestCommitPlacementViaGlobalAction()
        {
            Playfield playfield = null!;

            AddStep("select slider placement tool", () => InputManager.Key(Key.Number3));
            AddStep("move mouse to top left of playfield", () =>
            {
                playfield = this.ChildrenOfType<Playfield>().Single();
                var location = (3 * playfield.ScreenSpaceDrawQuad.TopLeft + playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("begin placement", () => InputManager.Click(MouseButton.Left));
            AddStep("move mouse to bottom right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopLeft + 3 * playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("confirm via global action", () =>
            {
                globalActionContainer.TriggerPressed(GlobalAction.Select);
                globalActionContainer.TriggerReleased(GlobalAction.Select);
            });
            AddAssert("slider placed", () => EditorBeatmap.HitObjects.Count, () => Is.EqualTo(1));
        }

        [Test]
        public void TestAbortPlacementViaGlobalAction()
        {
            Playfield playfield = null!;

            AddStep("select slider placement tool", () => InputManager.Key(Key.Number3));
            AddStep("move mouse to top left of playfield", () =>
            {
                playfield = this.ChildrenOfType<Playfield>().Single();
                var location = (3 * playfield.ScreenSpaceDrawQuad.TopLeft + playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("begin placement", () => InputManager.Click(MouseButton.Left));
            AddStep("move mouse to bottom right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopLeft + 3 * playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("abort via global action", () =>
            {
                globalActionContainer.TriggerPressed(GlobalAction.Back);
                globalActionContainer.TriggerReleased(GlobalAction.Back);
            });
            AddAssert("editor is still current", () => Editor.IsCurrentScreen());
            AddAssert("slider not placed", () => EditorBeatmap.HitObjects.Count, () => Is.EqualTo(0));
            AddAssert("no active placement", () => this.ChildrenOfType<ComposeBlueprintContainer>().Single().CurrentPlacement.PlacementActive,
                () => Is.EqualTo(PlacementBlueprint.PlacementState.Waiting));
        }

        [Test]
        public void TestCommitPlacementViaToolChange()
        {
            Playfield playfield = null!;

            AddStep("select slider placement tool", () => InputManager.Key(Key.Number3));
            AddStep("move mouse to top left of playfield", () =>
            {
                playfield = this.ChildrenOfType<Playfield>().Single();
                var location = (3 * playfield.ScreenSpaceDrawQuad.TopLeft + playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("begin placement", () => InputManager.Click(MouseButton.Left));
            AddStep("move mouse to bottom right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopLeft + 3 * playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });

            AddStep("change tool to circle", () => InputManager.Key(Key.Number2));
            AddAssert("slider placed", () => EditorBeatmap.HitObjects.Count, () => Is.EqualTo(1));
        }
    }
}
