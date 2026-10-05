// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Osu
{
    public static class OsuEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Osu.OsuEditor";

        /// <summary>
        /// "Hit circle"
        /// </summary>
        public static LocalisableString HitCircleTool => new TranslatableString(getKey(@"hit_circle_tool"), @"Hit circle");

        /// <summary>
        /// "Slider"
        /// </summary>
        public static LocalisableString SliderTool => new TranslatableString(getKey(@"slider_tool"), @"Slider");

        /// <summary>
        /// "Spinner"
        /// </summary>
        public static LocalisableString SpinnerTool => new TranslatableString(getKey(@"spinner_tool"), @"Spinner");

        /// <summary>
        /// "Grid"
        /// </summary>
        public static LocalisableString GridFromPointsTool => new TranslatableString(getKey(@"grid_from_points_tool"), @"Grid");

        /// <summary>
        /// "Toggle position snap grid"
        /// </summary>
        public static LocalisableString ToggleGridSnap => new TranslatableString(getKey(@"toggle_grid_snap"), @"Toggle position snap grid");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
