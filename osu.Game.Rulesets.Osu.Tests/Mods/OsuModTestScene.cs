// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public abstract partial class OsuModTestScene : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();
    }
}
