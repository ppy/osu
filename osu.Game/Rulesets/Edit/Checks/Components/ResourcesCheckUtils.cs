// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public static class ResourcesCheckUtils
    {
        /// <summary>
        /// Checks if any storyboard element is present in the working beatmap.
        /// </summary>
        /// <param name="workingBeatmap">The working beatmap to check.</param>
        /// <returns>True if any storyboard element is present, false otherwise.</returns>
        public static bool HasAnyStoryboardElementPresent(IWorkingBeatmap workingBeatmap)
        {
            foreach (var layer in workingBeatmap.Storyboard.Layers)
            {
                foreach (var _ in layer.Elements)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
