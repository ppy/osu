// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that applies changes to a <see cref="BeatmapConverter{TObject}"/>.
    /// </summary>
    /// <typeparam name="TObject">The type of converted <see cref="HitObject"/>.</typeparam>
    public interface IApplicableToBeatmapConverter : IApplicableMod
    {
        /// <summary>
        /// Applies this <see cref="Mod"/> to a <see cref="BeatmapConverter{TObject}"/>.
        /// </summary>
        /// <param name="beatmapConverter">The <see cref="BeatmapConverter{TObject}"/> to apply to.</param>
        void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter);
    }
}
