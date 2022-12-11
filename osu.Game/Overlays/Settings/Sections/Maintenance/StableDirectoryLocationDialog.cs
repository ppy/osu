// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Overlays.Dialog;
using osu.Game.Screens;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class StableDirectoryLocationDialog : PopupDialog
    {
        [Resolved]
        private IPerformFromScreenRunner performer { get; set; }

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
                    Action = () => Schedule(() => performer.PerformFromScreen(screen => screen.Push(new StableDirectorySelectScreen(taskCompletionSource))))
                },
                new PopupDialogCancelButton
                {
                    Text = "其实我没有装stable",
                    Action = () => taskCompletionSource.TrySetCanceled()
                }
            };
        }
    }
}
