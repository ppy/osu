using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Skinning;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class SongProgressButton : BottomBarSwitchButton
    {
        protected override string BackgroundTextureName => "MButtonProgressOff-background";
        protected override string SwitchOnBgTextureName => "MButtonProgressOn-background";
        protected override ConfineMode TextureConfineMode => ConfineMode.NoScaling;

        private string timeCurrent;
        private string timeTotal;

        [Resolved]
        private MusicController musicController { get; set; }

        private string formatTime(TimeSpan timeSpan) => $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{Math.Floor(timeSpan.Duration().TotalMinutes)}:{timeSpan.Duration().Seconds:D2}";

        public SongProgressButton()
        {
            TooltipText = "切换暂停";
            AutoSizeAxes = Axes.X;
            SpriteText.UseLegacyUnicode = true;
        }

        protected override void Update()
        {
            base.Update();

            var track = musicController.CurrentTrack;
            int currentSecond = (int)Math.Floor(track.CurrentTime / 1000.0);
            timeCurrent = formatTime(TimeSpan.FromSeconds(currentSecond));
            timeTotal = formatTime(TimeSpan.FromMilliseconds(track.Length));
            Text = $"{timeCurrent} / {timeTotal}";
        }
    }
}
