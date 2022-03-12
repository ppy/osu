// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
            HeaderText = "无法定位osu!stable安装路径";
            BodyText = "如果您知道它在哪，请手动选择一个路径来导入。";
            Icon = FontAwesome.Solid.QuestionCircle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = "我当然知道它在哪！",
                    Action = () => Schedule(() => game.PerformFromScreen(screen => screen.Push(new StableDirectorySelectScreen(taskCompletionSource))))
                },
                new PopupDialogCancelButton
                {
                    Text = "我其实没有装",
                    Action = () => taskCompletionSource.TrySetCanceled()
                }
            };
        }
    }
}
