// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.IO;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public class StorageErrorDialog : PopupDialog
    {
        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; }

        public StorageErrorDialog(OsuStorage storage, OsuStorageError error)
        {
            HeaderText = "存储错误!";
            Icon = FontAwesome.Solid.ExclamationTriangle;

            var buttons = new List<PopupDialogButton>();

            switch (error)
            {
                case OsuStorageError.NotAccessible:
                    BodyText = $"我们无法访问 (\"{storage.CustomStoragePath}\"). 请检查您的权限设置, 如果这是一个外接设备, 请尝试重新插入该设备.";

                    buttons.AddRange(new PopupDialogButton[]
                    {
                        new PopupDialogCancelButton
                        {
                            Text = "重试",
                            Action = () =>
                            {
                                if (!storage.TryChangeToCustomStorage(out var nextError))
                                    dialogOverlay.Push(new StorageErrorDialog(storage, nextError));
                            }
                        },
                        new PopupDialogCancelButton
                        {
                            Text = "在下次启动前使用默认地址",
                        },
                        new PopupDialogOkButton
                        {
                            Text = "重置为默认地址",
                            Action = storage.ResetCustomStoragePath
                        },
                    });
                    break;

                case OsuStorageError.AccessibleButEmpty:
                    BodyText = $"目录 (\"{storage.CustomStoragePath}\") 为空. 如果您移动了该目录下的文件, 请将这些文件移动回来并重启osu!";

                    // Todo: Provide the option to search for the files similar to migration.
                    buttons.AddRange(new PopupDialogButton[]
                    {
                        new PopupDialogCancelButton
                        {
                            Text = "刷新"
                        },
                        new PopupDialogOkButton
                        {
                            Text = "重置为默认地址",
                            Action = storage.ResetCustomStoragePath
                        },
                    });

                    break;
            }

            Buttons = buttons;
        }
    }
}
