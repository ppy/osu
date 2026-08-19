// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Catch
{
    public static class CatchEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Catch.CatchEditor";

        /// <summary>
        /// "Fruit"
        /// </summary>
        public static LocalisableString FruitTool => new TranslatableString(getKey(@"fruit_tool"), @"Fruit");

        /// <summary>
        /// "Juice stream"
        /// </summary>
        public static LocalisableString JuiceStreamTool => new TranslatableString(getKey(@"juice_stream_tool"), @"Juice stream");

        /// <summary>
        /// "Banana shower"
        /// </summary>
        public static LocalisableString BananaShowerTool => new TranslatableString(getKey(@"banana_shower_tool"), @"Banana shower");

        /// <summary>
        /// "Toggle distance snap grid"
        /// </summary>
        public static LocalisableString ToggleDistanceSnap => new TranslatableString(getKey(@"toggle_distance_snap"), @"Toggle distance snap grid");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
