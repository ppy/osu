// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Wraps the contents of the editor clipboard.
    /// </summary>
    public class EditorClipboard
    {
        public Bindable<string> Content { get; } = new Bindable<string>();
    }
}
