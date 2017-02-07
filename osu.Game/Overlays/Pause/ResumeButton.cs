using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Pause
{
    public class ResumeButton : PauseButton
    {
        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            ButtonColour = colours.Green;
            SampleHover = audio.Sample.Get(@"Menu/menuclick");
            SampleClick = audio.Sample.Get(@"Menu/menuback");
        }

        public ResumeButton()
        {
            Text = @"Continue";
        }
    }
}
