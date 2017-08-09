// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that are applied to a RulesetContainer.
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject the RulesetContainer contains.</typeparam>
    public interface IApplicableMod<TObject>
        where TObject : HitObject
    {
        /// <summary>
        /// Applies the mod to a RulesetContainer.
        /// </summary>
        /// <param name="rulesetContainer">The RulesetContainer to apply the mod to.</param>
        void ApplyToRulesetContainer(RulesetContainer<TObject> rulesetContainer);
    }
}
