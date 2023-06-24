// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// Created for a <see cref="DrawableHitObject"/> when only <see cref="HitObject"/> is given
    /// to make sure a <see cref="DrawableHitObject"/> is always associated with a <see cref="HitObjectLifetimeEntry"/>.
    /// </summary>
    internal class SyntheticHitObjectEntry : HitObjectLifetimeEntry
    {
        public SyntheticHitObjectEntry(HitObject hitObject)
            : base(hitObject)
        {
        }
    }
}
