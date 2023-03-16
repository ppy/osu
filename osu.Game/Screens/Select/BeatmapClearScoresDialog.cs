// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;
using osu.Game.Scoring;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapClearScoresDialog : DangerousActionDialog
    {
        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        public BeatmapClearScoresDialog(BeatmapInfo beatmapInfo, Action onCompletion)
        {
            BodyText = $"All local scores on {beatmapInfo.GetDisplayTitle()}";
            DangerousAction = () =>
            {
                Task.Run(() => scoreManager.Delete(beatmapInfo))
                    .ContinueWith(_ => onCompletion);
            };
        }
    }
}
