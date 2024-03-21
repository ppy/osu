// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public abstract partial class TaikoSkinnableTestScene : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new TaikoRuleset();
    }
}
