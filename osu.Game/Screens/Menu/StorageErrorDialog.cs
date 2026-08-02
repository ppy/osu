// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public partial class StorageErrorDialog : PopupDialog
    {
        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        public StorageErrorDialog(OsuStorage storage, OsuStorageError error)
        {
            HeaderText = StorageErrorDialogStrings.StorageError;
            Icon = FontAwesome.Solid.ExclamationTriangle;

            var buttons = new List<PopupDialogButton>();

            switch (error)
            {
                case OsuStorageError.NotAccessible:
                    BodyText = StorageErrorDialogStrings.LocationIsNotAccessible(storage.CustomStoragePath);

                    buttons.AddRange(new PopupDialogButton[]
                    {
                        new PopupDialogCancelButton
                        {
                            Text = StorageErrorDialogStrings.TryAgain,
                            Action = () =>
                            {
                                bool success;
                                OsuStorageError nextError;

                                // blocking all operations has a side effect of closing & reopening the realm db,
                                // which is desirable here since the restoration of the old storage - if it succeeds - means the realm db has moved.
                                using (realmAccess.BlockAllOperations(@"restoration of previously unavailable storage"))
                                    success = storage.TryChangeToCustomStorage(out nextError);

                                if (!success)
                                    dialogOverlay.Push(new StorageErrorDialog(storage, nextError));
                            }
                        },
                        new PopupDialogCancelButton
                        {
                            Text = StorageErrorDialogStrings.UseDefaultLocation,
                        },
                        new PopupDialogOkButton
                        {
                            Text = StorageErrorDialogStrings.ResetToDefaultLocation,
                            Action = storage.ResetCustomStoragePath
                        },
                    });
                    break;

                case OsuStorageError.AccessibleButEmpty:
                    BodyText = StorageErrorDialogStrings.LocationIsEmpty(storage.CustomStoragePath);

                    // Todo: Provide the option to search for the files similar to migration.
                    buttons.AddRange(new PopupDialogButton[]
                    {
                        new PopupDialogCancelButton
                        {
                            Text = StorageErrorDialogStrings.StartFresh
                        },
                        new PopupDialogOkButton
                        {
                            Text = StorageErrorDialogStrings.ResetToDefaultLocation,
                            Action = storage.ResetCustomStoragePath
                        },
                    });

                    break;
            }

            Buttons = buttons;
        }
    }
}
