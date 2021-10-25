// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Beatmap info retrieved for previewing locally.
    /// </summary>
    public interface IBeatmapOnlineInfo
    {
        int? MaxCombo { get; }

        BeatmapMetrics? Metrics { get; }
    }
}
