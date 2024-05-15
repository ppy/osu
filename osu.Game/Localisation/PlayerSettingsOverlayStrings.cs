// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class PlayerSettingsOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PlaybackSettings";

        /// <summary>
        /// "Step backward one frame"
        /// </summary>
        public static LocalisableString StepBackward => new TranslatableString(getKey(@"step_backward_frame"), @"Step backward one frame");

        /// <summary>
        /// "Step forward one frame"
        /// </summary>
        public static LocalisableString StepForward => new TranslatableString(getKey(@"step_forward_frame"), @"Step forward one frame");

        /// <summary>
        /// "Seek backward {0} seconds"
        /// </summary>
        public static LocalisableString SeekBackwardSeconds(double arg0) => new TranslatableString(getKey(@"seek_backward_seconds"), @"Seek backward {0} seconds", arg0);

        /// <summary>
        /// "Seek forward {0} seconds"
        /// </summary>
        public static LocalisableString SeekForwardSeconds(double arg0) => new TranslatableString(getKey(@"seek_forward_seconds"), @"Seek forward {0} seconds", arg0);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
