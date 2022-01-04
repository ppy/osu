// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Desktop.LegacyIpc
{
    /// <summary>
    /// A difficulty calculation response returned to the legacy client.
    /// </summary>
    /// <remarks>
    /// Synchronise any changes with osu!stable.
    /// </remarks>
    public class LegacyIpcDifficultyCalculationResponse
    {
        public double StarRating { get; set; }
    }
}
