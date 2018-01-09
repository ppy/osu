// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for <see cref="Mod"/>s that can be applied to <see cref="HitObject"/>s.
    /// </summary>
    public interface IApplicableToHitObject<in TObject> : IApplicableMod
        where TObject : HitObject
    {
        /// <summary>
        /// Applies this <see cref="IApplicableToHitObject{TObject}"/> to a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to apply to.</param>
        void ApplyToHitObject(TObject hitObject);
    }
}
