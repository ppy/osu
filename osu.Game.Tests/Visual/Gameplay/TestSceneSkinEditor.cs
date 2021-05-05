// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning.Editor;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinEditor : PlayerTestScene
    {
        private SkinEditor skinEditor;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add editor overlay", () =>
            {
                skinEditor?.Expire();
                LoadComponentAsync(skinEditor = new SkinEditor(Player), Add);
            });

            AddUntilStep("wait for loaded", () => skinEditor.IsLoaded);
        }

        [Test]
        public void TestToggleEditor()
        {
            AddToggleStep("toggle editor visibility", visible => skinEditor.ToggleVisibility());
        }

        [Test]
        public void TestSelectionLostOnEditorClose()
        {
            SkinBlueprintContainer skinBlueprintContainer = null;

            AddStep("show editor", () => skinEditor.State.Value = Visibility.Visible);
            AddStep("retrieve blueprint container", () => skinBlueprintContainer = this.ChildrenOfType<SkinBlueprintContainer>().Single());

            AddStep("select all blueprints", () => skinBlueprintContainer.SelectAll());
            AddAssert("all blueprints selected", () => skinBlueprintContainer.SelectionBlueprints.All(blueprint => blueprint.IsSelected));

            AddStep("hide editor", () => skinEditor.State.Value = Visibility.Hidden);
            AddUntilStep("no blueprint selected", () => skinBlueprintContainer.SelectionBlueprints.All(blueprint => !blueprint.IsSelected));
        }

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();
    }
}
