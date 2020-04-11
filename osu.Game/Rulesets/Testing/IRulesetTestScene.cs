// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Testing
{
    /// <summary>
    /// An interface that can be assigned to test scenes to indicate
    /// that the test scene is testing ruleset-specific components.
    /// This is to cache required ruleset dependencies for the components.
    /// </summary>
    public interface IRulesetTestScene
    {
        /// <summary>
        /// Retrieves the ruleset that is going
        /// to be tested by this test scene.
        /// </summary>
        /// <returns>The <see cref="Ruleset"/>.</returns>
        Ruleset CreateRuleset();
    }
}
