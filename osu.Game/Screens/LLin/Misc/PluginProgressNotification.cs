using System;
using osu.Game.Overlays.Notifications;

#nullable disable

namespace osu.Game.Screens.LLin.Misc
{
    public partial class PluginProgressNotification : ProgressNotification
    {
        public Action OnComplete { get; set; }

        public override void Close(bool runFlingAnimation)
        {
            OnComplete?.Invoke();
            base.Close(runFlingAnimation);
        }
    }
}
