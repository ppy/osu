// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Structure used to transport data between <see cref="Editor"/> instances on difficulty change.
    /// It's intended to be received by <see cref="EditorLoader"/> from one editor instance and passed down to the next one.
    /// </summary>
    public class EditorState
    {
        /// <summary>
        /// The current clock time when a difficulty switch was requested.
        /// </summary>
        public double? Time { get; set; }

        /// <summary>
        /// The current editor clipboard content at the time when a difficulty switch was requested.
        /// </summary>
        public string? ClipboardContent { get; set; }
    }
}
