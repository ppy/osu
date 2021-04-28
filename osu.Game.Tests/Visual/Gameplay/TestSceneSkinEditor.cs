// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Skinning.Editor;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinEditor : SkinnableTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create editor overlay", () =>
            {
                SetContents(() =>
                {
                    var hudOverlay = new HUDOverlay(null, null, null, Array.Empty<Mod>());

                    // Add any key just to display the key counter visually.
                    hudOverlay.KeyCounter.Add(new KeyCounterKeyboard(Key.Space));
                    hudOverlay.ComboCounter.Current.Value = 1;

                    return new SkinEditor(hudOverlay);
                });
            });
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
