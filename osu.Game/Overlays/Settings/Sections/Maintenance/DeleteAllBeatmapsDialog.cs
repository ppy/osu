// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class DeleteAllBeatmapsDialog : PopupDialog
    {
        private BeatmapManager manager;

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            manager = beatmapManager;
        }

        public DeleteAllBeatmapsDialog(Action deleteAction)
        {
            BodyText = "Everything?";

            Icon = FontAwesome.fa_trash_o;
            HeaderText = @"Confirm deletion of";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Go for it.",
                    Action = deleteAction
                },
                new PopupDialogCancelButton
                {
                    Text = @"No! Abort mission!",
                },
            };
        }
    }
}
