// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneTaikoPlayer : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new TaikoRuleset();
    }
}
