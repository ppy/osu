using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Pause
{
    public class RetryButton : PauseButton
    {
        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            ButtonColour = colours.YellowDark;
            SampleHover = audio.Sample.Get(@"Menu/menuclick");
            SampleClick = audio.Sample.Get(@"Menu/menu-play-click");
        }

        public RetryButton()
        {
            Text = @"Retry";
        }
    }
}
