// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneContextualExpandingToolbox : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        private HitObjectComposer composer => Editor.ChildrenOfType<HitObjectComposer>().First();

        private ContextualExpandingToolboxContainer rightToolbox => Editor.ChildrenOfType<ContextualExpandingToolboxContainer>().First();

        private HitObjectCompositionToolButton toolButton<T>() where T : CompositionTool => composer.ChildrenOfType<EditorRadioButton>()
                                                                                                    .Select(it => it.Button as HitObjectCompositionToolButton)
                                                                                                    .OfType<HitObjectCompositionToolButton>()
                                                                                                    .First(it => it.Tool is T);

        [Test]
        public void TestContextualToolboxGroupPresence()
        {
            AddStep("add slider toolbox group", () => rightToolbox.ContextualToolboxGroup = new FreehandSliderToolboxGroup());
            AddAssert("slider toolbox group is present", () => rightToolbox.ChildrenOfType<FreehandSliderToolboxGroup>().Count() == 1);

            AddStep("remove slider toolbox group", () => rightToolbox.ContextualToolboxGroup = null);
            AddAssert("slider toolbox group is not present", () => !rightToolbox.ChildrenOfType<FreehandSliderToolboxGroup>().Any());
        }

        [Test]
        public void TestContextualToolboxGroupActivationOnToolSwitch()
        {
            AddStep("select slider tool", () => toolButton<SliderCompositionTool>().Select());
            AddAssert("slider toolbox group visible", () => rightToolbox.ContextualToolboxGroup is FreehandSliderToolboxGroup);

            AddStep("select circle tool", () => toolButton<HitCircleCompositionTool>().Select());
            AddAssert("slider toolbox group not visible", () => rightToolbox.ContextualToolboxGroup == null);
        }
    }
}
