using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Import
{
    public class ImportConfirmDialog : PopupDialog
    {
        public Action OnConfirmedAction;

        public ImportConfirmDialog(string fileName)
        {
            Icon = FontAwesome.Regular.QuestionCircle;
            HeaderText = "您确定吗?";
            BodyText = $"即将导入文件: {fileName}";

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = "确定",
                    Action = () => OnConfirmedAction?.Invoke()
                },
                new PopupDialogCancelButton
                {
                    Text = "取消",
                },
            };
        }
    }
}