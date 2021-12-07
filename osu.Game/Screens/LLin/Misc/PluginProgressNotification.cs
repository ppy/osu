using System;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Screens.LLin.Misc
{
    public class PluginProgressNotification : ProgressNotification
    {
        public Action OnComplete { get; set; }

        protected override void Completed()
        {
            OnComplete?.Invoke();
            base.Completed();
        }
    }
}
