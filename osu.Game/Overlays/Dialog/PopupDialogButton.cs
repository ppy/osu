// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Dialog
{
    public partial class PopupDialogButton : DialogButton
    {
        /// <summary>
        /// Whether the dialog should be closed before the action related to this button is invoked.
        /// </summary>
        /// <remarks>
        /// This is important as the code which is performed may check for a dialog being present (ie. `OsuGame.PerformFromScreen`)
        /// and we don't want it to see the already dismissed dialog.
        /// </remarks>
        public virtual bool HideDialogBeforeInvoke => true;

        public PopupDialogButton(HoverSampleSet sampleSet = HoverSampleSet.Button)
            : base(sampleSet)
        {
            Height = 50;
            TextSize = 18;
        }
    }
}
