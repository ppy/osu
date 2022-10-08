using System;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Screens.LLin.Misc
{
    public class PluginProgressNotification : ProgressNotification
    {
        public Action OnComplete { get; set; }

        public override void Close(bool runFlingAnimation)
        {
            OnComplete?.Invoke();
            base.Close(runFlingAnimation);
        }
    }
}
