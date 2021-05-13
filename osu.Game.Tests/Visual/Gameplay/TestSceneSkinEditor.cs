// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning;
using osu.Game.Skinning.Editor;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinEditor : PlayerTestScene
    {
        private SkinEditor skinEditor;

        [Resolved]
        private SkinManager skinManager { get; set; }

        protected override bool Autoplay => true;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reload skin editor", () =>
            {
                skinEditor?.Expire();
                Player.ScaleTo(SkinEditorOverlay.VISIBLE_TARGET_SCALE);
                LoadComponentAsync(skinEditor = new SkinEditor(Player), Add);
            });
        }

        [Test]
        public void TestToggleEditor()
        {
            AddToggleStep("toggle editor visibility", visible => skinEditor.ToggleVisibility());
        }

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();
    }
}
