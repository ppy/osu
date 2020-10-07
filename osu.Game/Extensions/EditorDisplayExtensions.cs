// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Extensions
{
    public static class EditorDisplayExtensions
    {
        /// <summary>
        /// Get an editor formatted string (mm:ss:mss)
        /// </summary>
        /// <param name="milliseconds">A time value in milliseconds.</param>
        /// <returns>An editor formatted display string.</returns>
        public static string ToEditorFormattedString(this double milliseconds) =>
            ToEditorFormattedString(TimeSpan.FromMilliseconds(milliseconds));

        /// <summary>
        /// Get an editor formatted string (mm:ss:mss)
        /// </summary>
        /// <param name="timeSpan">A time value.</param>
        /// <returns>An editor formatted display string.</returns>
        public static string ToEditorFormattedString(this TimeSpan timeSpan) =>
            $"{(timeSpan < TimeSpan.Zero ? "-" : string.Empty)}{timeSpan:mm\\:ss\\:fff}";
    }
}
