using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Audio;
using osu.Game.Screens.LLin.Plugins.Types;

namespace osu.Game.Screens.LLin.Plugins.Internal.FallbackFunctionBar
{
    public class SongProgressButton : ToggleableBarButton
    {
        public SongProgressButton(IToggleableFunctionProvider provider)
            : base(provider)
        {
            Width = 120;
        }

        private string timeCurrent;
        private string timeTotal;

        [Resolved]
        private IImplementLLin mvisScreen { get; set; }

        private DrawableTrack track => mvisScreen.CurrentTrack;

        private string formatTime(TimeSpan timeSpan) => $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{Math.Floor(timeSpan.Duration().TotalMinutes)}:{timeSpan.Duration().Seconds:D2}";

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
