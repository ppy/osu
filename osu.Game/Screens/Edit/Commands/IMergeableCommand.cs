// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;

namespace osu.Game.Screens.Edit.Commands
{
    /// <summary>
    /// An editor command which can be merged with a previous command. Useful for reducing the amount of
    /// commands stored in the undo stack.
    /// </summary>
    public interface IMergeableCommand : IEditorCommand
    {
        /// <summary>
        /// Attempts to merge this command with a command that has previously been applied.
        /// The resulting command should have the same effect as applying both commands in sequence.
        /// </summary>
        /// <param name="previousCommand">The previously applied command</param>
        /// <param name="merged">The resulting command, or null if the commands cannot be merged.</param>
        public bool MergeWithPrevious(IEditorCommand previousCommand, [MaybeNullWhen(false)] out IEditorCommand merged);
    }

    public static class MergeableCommandExtensions
    {
        /// <summary>
        /// Attempts to merge this command with a command that will be applied after it.
        /// The resulting command should have the same effect as applying both commands in sequence.
        /// </summary>
        /// <param name="current">The command being merged</param>
        /// <param name="nextCommand">The command applied after this one</param>
        /// <param name="merged">The resulting command, or null if the commands cannot be merged.</param>
        public static bool MergeWithNext(this IMergeableCommand current, IEditorCommand nextCommand, [MaybeNullWhen(false)] out IEditorCommand merged)
        {
            if (nextCommand is IMergeableCommand mergeable)
                return mergeable.MergeWithPrevious(current, out merged);

            merged = null;
            return false;
        }
    }
}
