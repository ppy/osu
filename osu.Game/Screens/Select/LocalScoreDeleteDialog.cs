// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Overlays.Dialog;
using osu.Game.Scoring;
using System.Diagnostics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select
{
    public class LocalScoreDeleteDialog : PopupDialog
    {
        private readonly ScoreInfo score;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        public LocalScoreDeleteDialog(ScoreInfo score)
        {
            this.score = score;
            Debug.Assert(score != null);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BeatmapInfo beatmap = beatmapManager.QueryBeatmap(b => b.ID == score.BeatmapInfoID);
            Debug.Assert(beatmap != null);

            string accuracy = string.Format(score.Accuracy == 1 ? "{0:0%}" : "{0:0.00%}", score.Accuracy);
            BodyText = $"玩家:{score.User} \n 准确率{accuracy}, 评级{score.Rank}, 最大连击{score.MaxCombo}, 总分{score.TotalScore}";

            Icon = FontAwesome.Regular.TrashAlt;
            HeaderText = "请确认是否删除这个成绩?";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = "是的",
                    Action = () => scoreManager?.Delete(score)
                },
                new PopupDialogCancelButton
                {
                    Text = "我需要再想想",
                },
            };
        }
    }
}