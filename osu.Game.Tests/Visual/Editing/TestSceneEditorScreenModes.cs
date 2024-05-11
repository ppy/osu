// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneEditorScreenModes : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestSwitchScreensInstantaneously()
        {
            AddStep("switch between all screens at once", () =>
            {
                foreach (var screen in Enum.GetValues(typeof(EditorScreenMode)).Cast<EditorScreenMode>())
                    Editor.Mode.Value = screen;
            });
        }
    }
}
