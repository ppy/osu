// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Structure used to convey the general state of an <see cref="Editor"/> instance.
    /// </summary>
    public class EditorState
    {
        /// <summary>
        /// The current audio time.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// The editor clipboard content.
        /// </summary>
        public string ClipboardContent { get; set; } = string.Empty;
    }
}
