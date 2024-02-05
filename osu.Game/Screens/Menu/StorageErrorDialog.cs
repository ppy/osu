// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.IO;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public partial class StorageErrorDialog : PopupDialog
    {
        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        public StorageErrorDialog(OsuStorage storage, OsuStorageError error)
        {
            HeaderText = "osu! storage error";
            Icon = FontAwesome.Solid.ExclamationTriangle;

            var buttons = new List<PopupDialogButton>();

            switch (error)
            {
                case OsuStorageError.NotAccessible:
                    BodyText = $"The specified osu! data location (\"{storage.CustomStoragePath}\") is not accessible. If it is on external storage, please reconnect the device and try again.";

                    buttons.AddRange(new PopupDialogButton[]
                    {
                        new PopupDialogCancelButton
                        {
                            Text = "Try again",
                            Action = () =>
                            {
                                if (!storage.TryChangeToCustomStorage(out var nextError))
                                    dialogOverlay.Push(new StorageErrorDialog(storage, nextError));
                            }
                        },
                        new PopupDialogCancelButton
                        {
                            Text = "Use default location until restart",
                        },
                        new PopupDialogOkButton
                        {
                            Text = "Reset to default location",
                            Action = storage.ResetCustomStoragePath
                        },
                    });
                    break;

                case OsuStorageError.AccessibleButEmpty:
                    BodyText = $"The specified osu! data location (\"{storage.CustomStoragePath}\") is empty. If you have moved the files, please close osu! and move them back.";

                    // Todo: Provide the option to search for the files similar to migration.
                    buttons.AddRange(new PopupDialogButton[]
                    {
                        new PopupDialogCancelButton
                        {
                            Text = "Start fresh at specified location"
                        },
                        new PopupDialogOkButton
                        {
                            Text = "Reset to default location",
                            Action = storage.ResetCustomStoragePath
                        },
                    });

                    break;
            }

            Buttons = buttons;
        }
    }
}
