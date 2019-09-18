// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Provides functionality to alter a <see cref="IBeatmap"/> after it has been converted.
    /// </summary>
    public interface IBeatmapProcessor
    {
        /// <summary>
        /// The <see cref="IBeatmap"/> to process. This should already be converted to the applicable <see cref="Ruleset"/>.
        /// </summary>
        IBeatmap Beatmap { get; }

        /// <summary>
        /// Processes the converted <see cref="Beatmap"/> prior to <see cref="HitObject.ApplyDefaults"/> being invoked.
        /// <para>
        /// Nested <see cref="HitObject"/>s generated during <see cref="HitObject.ApplyDefaults"/> will not be present by this point,
        /// and no mods will have been applied to the <see cref="HitObject"/>s.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This can only be used to add alterations to <see cref="HitObject"/>s generated directly through the conversion process.
        /// </remarks>
        void PreProcess();

        /// <summary>
        /// Processes the converted <see cref="Beatmap"/> after <see cref="HitObject.ApplyDefaults"/> has been invoked.
        /// <para>
        /// Nested <see cref="HitObject"/>s generated during <see cref="HitObject.ApplyDefaults"/> will be present by this point,
        /// and mods will have been applied to all <see cref="HitObject"/>s.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This should be used to add alterations to <see cref="HitObject"/>s while they are in their most playable state.
        /// </remarks>
        void PostProcess();
    }
}
