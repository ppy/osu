// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;
using osu.Game.Scoring;

namespace osu.Game.Screens.Select
{
    public class BeatmapClearScoresDialog : PopupDialog
    {
        [Resolved]
        private ScoreManager scoreManager { get; set; }

        public BeatmapClearScoresDialog(BeatmapInfo beatmapInfo, Action onCompletion)
        {
            BodyText = beatmapInfo.GetDisplayTitle();
            Icon = FontAwesome.Solid.Eraser;
            HeaderText = @"Clearing all local scores. Are you sure?";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Please.",
                    Action = () =>
                    {
                        Task.Run(() => scoreManager.Delete(beatmapInfo))
                            .ContinueWith(_ => onCompletion);
                    }
                },
                new PopupDialogCancelButton
                {
                    Text = @"No, I'm still attached.",
                },
            };
        }
    }
}
