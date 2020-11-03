using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class SongProgressButton : BottomBarSwitchButton
    {
        private string timeCurrent;
        private string timeTotal;

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

        private string formatTime(TimeSpan timeSpan) => $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{Math.Floor(timeSpan.Duration().TotalMinutes)}:{timeSpan.Duration().Seconds:D2}";

        public SongProgressButton()
        {
            TooltipText = "切换暂停";
            NoIcon = true;
        }

        protected override void Update()
        {
            base.Update();

            var Track = b.Value?.TrackLoaded ?? false ? b.Value.Track : null;
            if (Track?.IsDummyDevice == false)
            {
                int currentSecond = (int)Math.Floor(Track.CurrentTime / 1000.0);
                timeCurrent = formatTime(TimeSpan.FromSeconds(currentSecond));
                timeTotal = formatTime(TimeSpan.FromMilliseconds(b.Value.Track.Length));
            }
            else
            {
                timeCurrent = "???";
                timeTotal = "???";
            }

            Text = $"{timeCurrent} / {timeTotal}";
        }
    }
}
