// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;

namespace osu.Game.Rulesets.Replays.Types
{
    /// <summary>
    /// A type of <see cref="ReplayFrame"/> which can be converted from a <see cref="LegacyReplayFrame"/>.
    /// </summary>
    public interface IConvertibleReplayFrame
    {
        /// <summary>
        /// Populates this <see cref="ReplayFrame"/> using values from a <see cref="LegacyReplayFrame"/>.
        /// </summary>
        /// <param name="currentFrame">The <see cref="LegacyReplayFrame"/> to extract values from.</param>
        /// <param name="beatmap">The beatmap.</param>
        /// <param name="lastFrame">The last post-conversion <see cref="ReplayFrame"/>, used to fill in missing delta information. May be null.</param>
        void ConvertFrom(LegacyReplayFrame currentFrame, IBeatmap beatmap, ReplayFrame lastFrame = null);
    }
}
