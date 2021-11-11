// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorTestGameplay : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();
    }
}
