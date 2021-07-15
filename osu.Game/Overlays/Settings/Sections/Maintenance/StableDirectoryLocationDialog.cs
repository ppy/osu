﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class StableDirectoryLocationDialog : PopupDialog
    {
        [Resolved]
        private OsuGame game { get; set; }

        public StableDirectoryLocationDialog(TaskCompletionSource<string> taskCompletionSource)
        {
            HeaderText = "Failed to automatically locate an osu!stable installation.";
            BodyText = "An existing install could not be located. If you know where it is, you can help locate it.";
            Icon = FontAwesome.Solid.QuestionCircle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = "Sure! I know where it is located!",
                    Action = () => Schedule(() => game.PerformFromScreen(screen => screen.Push(new StableDirectorySelectScreen(taskCompletionSource))))
                },
                new PopupDialogCancelButton
                {
                    Text = "Actually I don't have osu!stable installed.",
                    Action = () => taskCompletionSource.TrySetCanceled()
                }
            };
        }
    }
}
