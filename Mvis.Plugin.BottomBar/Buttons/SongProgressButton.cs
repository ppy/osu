using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Game.Screens.LLin;
using osu.Game.Screens.LLin.Plugins.Types;

namespace Mvis.Plugin.BottomBar.Buttons
{
    public class SongProgressButton : BottomBarSwitchButton
    {
        private string timeCurrent;
        private string timeTotal;

        [Resolved]
        private IImplementLLin mvis { get; set; }

        private DrawableTrack track => mvis.CurrentTrack;

        private string formatTime(TimeSpan timeSpan) => $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{Math.Floor(timeSpan.Duration().TotalMinutes)}:{timeSpan.Duration().Seconds:D2}";

        public SongProgressButton(IToggleableFunctionProvider provider)
            : base(provider)
        {
            AutoSizeAxes = Axes.X;
        }

        protected override void Update()
        {
            base.Update();

            int currentSecond = (int)Math.Floor(track.CurrentTime / 1000.0);
            timeCurrent = formatTime(TimeSpan.FromSeconds(currentSecond));
            timeTotal = formatTime(TimeSpan.FromMilliseconds(track.Length));
            Title = $"{timeCurrent} / {timeTotal}";
        }
    }
}
