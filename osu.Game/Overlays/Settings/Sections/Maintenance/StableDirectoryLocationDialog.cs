// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;
using osu.Game.Screens;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class StableDirectoryLocationDialog : PopupDialog
    {
        [Resolved]
        private IPerformFromScreenRunner performer { get; set; }

        public StableDirectoryLocationDialog(TaskCompletionSource<string> taskCompletionSource)
        {
            HeaderText = MaintenanceSettingsStrings.StableDirectoryLocationHeader;
            BodyText = MaintenanceSettingsStrings.StableDirectoryLocationBody;
            Icon = FontAwesome.Solid.QuestionCircle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = "Sure! I know where it is located!",
                    Action = () => Schedule(() => performer.PerformFromScreen(screen => screen.Push(new StableDirectorySelectScreen(taskCompletionSource))))
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
