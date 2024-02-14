// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.Gameplay
{
    /// <summary>
    /// A <see cref="PlayerTestScene"/> with an arbitrary ruleset value to test with.
    /// </summary>
    public abstract partial class OsuPlayerTestScene : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();
    }
}
