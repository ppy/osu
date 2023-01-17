// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit
{
    public partial class CreateNewDifficultyDialog : PopupDialog
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
            HeaderText = EditorDialogsStrings.NewDifficultyDialogHeader;

            Icon = FontAwesome.Regular.Clone;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = EditorDialogsStrings.CreateNew,
                    Action = () => createNewDifficulty.Invoke(false)
                },
                new PopupDialogCancelButton
                {
                    Text = EditorDialogsStrings.CreateCopy,
                    Action = () => createNewDifficulty.Invoke(true)
                },
                new PopupDialogCancelButton
                {
                    Text = EditorDialogsStrings.KeepEditing,
                    Action = () => { }
                }
            };
        }
    }
}
