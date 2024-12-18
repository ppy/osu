// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public abstract partial class TaikoModTestScene : ModTestScene
    {
        protected sealed override Ruleset CreatePlayerRuleset() => new TaikoRuleset();
    }
}
