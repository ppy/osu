// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Collections
{
    public class DeleteCollectionDialog : PopupDialog
    {
        [Resolved]
        private CollectionManager collectionManager { get; set; }

        public DeleteCollectionDialog(BeatmapCollection collection)
        {
            HeaderText = "Confirm deletion of";
            BodyText = collection.Name;

            Icon = FontAwesome.Regular.TrashAlt;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Go for it.",
                    Action = () => collectionManager.Collections.Remove(collection)
                },
                new PopupDialogCancelButton
                {
                    Text = @"No! Abort mission!",
                },
            };
        }
    }
}
