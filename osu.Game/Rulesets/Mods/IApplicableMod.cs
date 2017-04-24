// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that are applied to a HitRenderer.
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject the HitRenderer contains.</typeparam>
    public interface IApplicableMod<TObject>
        where TObject : HitObject
    {
        /// <summary>
        /// Applies the mod to a HitRenderer.
        /// </summary>
        /// <param name="hitRenderer">The HitRenderer to apply the mod to.</param>
        void ApplyToHitRenderer(HitRenderer<TObject> hitRenderer);
    }
}
