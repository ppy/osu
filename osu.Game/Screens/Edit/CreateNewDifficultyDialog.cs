// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public class CreateNewDifficultyDialog : PopupDialog
    {
        /// <summary>
        /// Delegate used to create new difficulties.
        /// A value of <see langword="true"/> in the <c>createCopy</c> parameter
        /// indicates that the new difficulty should be an exact copy of an existing one;
        /// otherwise, the new difficulty should have its hitobjects and beatmap-level settings cleared.
        /// </summary>
        public delegate void CreateNewDifficulty(bool createCopy);

        public CreateNewDifficultyDialog(CreateNewDifficulty createNewDifficulty)
        {
            HeaderText = "Would you like to create a blank difficulty?";

            Icon = FontAwesome.Regular.Clone;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = "Yeah, let's start from scratch!",
                    Action = () => createNewDifficulty.Invoke(false)
                },
                new PopupDialogCancelButton
                {
                    Text = "No, create an exact copy of this difficulty",
                    Action = () => createNewDifficulty.Invoke(true)
                },
                new PopupDialogCancelButton
                {
                    Text = "I changed my mind, I want to keep editing this difficulty",
                    Action = () => { }
                }
            };
        }
    }
}
