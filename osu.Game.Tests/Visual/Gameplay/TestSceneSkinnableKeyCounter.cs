// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinnableKeyCounter : SkinnableHUDComponentTestScene
    {
        [Cached]
        private readonly InputCountController controller = new InputCountController();

        public override void SetUpSteps()
        {
            AddStep("create dependencies", () =>
            {
                Add(controller);
                controller.Add(new KeyCounterKeyboardTrigger(Key.Z));
                controller.Add(new KeyCounterKeyboardTrigger(Key.X));
                controller.Add(new KeyCounterKeyboardTrigger(Key.C));
                controller.Add(new KeyCounterKeyboardTrigger(Key.V));

                foreach (var trigger in controller.Triggers)
                    Add(trigger);
            });
            base.SetUpSteps();
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        protected override Drawable CreateArgonImplementation() => new ArgonKeyCounterDisplay();

        protected override Drawable CreateDefaultImplementation() => new DefaultKeyCounterDisplay();

        protected override Drawable CreateLegacyImplementation() => new LegacyKeyCounterDisplay();
    }
}
