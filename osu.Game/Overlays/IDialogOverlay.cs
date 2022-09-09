// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays
{
    /// <summary>
    /// A global overlay that can show popup dialogs.
    /// </summary>
    [Cached(typeof(IDialogOverlay))]
    public interface IDialogOverlay
    {
        /// <summary>
        /// Push a new dialog for display.
        /// </summary>
        /// <remarks>
        /// This will immediate dismiss any already displayed dialog (cancelling the action).
        /// If the dialog instance provided is already displayed, it will be a noop.
        /// </remarks>
        /// <param name="dialog">The dialog to be presented.</param>
        void Push(PopupDialog dialog);

        /// <summary>
        /// The currently displayed dialog, if any.
        /// </summary>
        PopupDialog? CurrentDialog { get; }
    }
}
