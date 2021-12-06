// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Desktop.LegacyIpc
{
    /// <summary>
    /// A difficulty calculation request from the legacy client.
    /// </summary>
    /// <remarks>
    /// Synchronise any changes with osu!stable.
    /// </remarks>
    public class LegacyIpcDifficultyCalculationRequest
    {
        public string BeatmapFile { get; set; }
        public int RulesetId { get; set; }
        public int Mods { get; set; }
    }
}
