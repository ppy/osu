// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that applies changes to a <see cref="Beatmap"/>
    /// after conversion and post-processing has completed.
    /// </summary>
    public interface IApplicableToBeatmap<TObject> : IApplicableMod
        where TObject : HitObject
    {
        /// <summary>
        /// Applies this <see cref="IApplicableToBeatmap{TObject}"/> to a <see cref="Beatmap{TObject}"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="Beatmap{TObject}"/> to apply to.</param>
        void ApplyToBeatmap(Beatmap<TObject> beatmap);
    }
}
