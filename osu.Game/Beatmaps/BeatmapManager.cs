// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Testing;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles general operations related to global beatmap management.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public class BeatmapManager
    {
        public BeatmapManager()
        {
            beatmapModelManager = new BeatmapModelManager()
        }

    }

}
