using System;
using osu.Framework.Allocation;
using osu.Game.Overlays;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class SongProgressButton : BottomBarSwitchButton
    {
        private string timeCurrent;
        private string timeTotal;

        [Resolved]
        private MusicController musicController { get; set; }

        private string formatTime(TimeSpan timeSpan) => $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{Math.Floor(timeSpan.Duration().TotalMinutes)}:{timeSpan.Duration().Seconds:D2}";

        public SongProgressButton()
        {
            TooltipText = "切换暂停";
            NoIcon = true;
        }

        protected override void Update()
        {
            base.Update();

            var Track = musicController.CurrentTrack;
            if (!Track.IsDummyDevice)
            {
                int currentSecond = (int)Math.Floor(Track.CurrentTime / 1000.0);
                timeCurrent = formatTime(TimeSpan.FromSeconds(currentSecond));
                timeTotal = formatTime(TimeSpan.FromMilliseconds(Track.Length));
                Text = $"{timeCurrent} / {timeTotal}";
            }
            else
            {
                Text = "请检查当前正在播放的内容";
            }
        }
    }
}
