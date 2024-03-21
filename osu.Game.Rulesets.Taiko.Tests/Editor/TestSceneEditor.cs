// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneEditor : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new TaikoRuleset();
    }
}
