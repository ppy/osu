// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that applies changes to a <see cref="Beatmap"/> after conversion and post-processing has completed without changing its difficulty
    /// </summary>
    public interface IApplicableToBeatmapKeepStarRating : IApplicableToBeatmap
    {
    }
}
