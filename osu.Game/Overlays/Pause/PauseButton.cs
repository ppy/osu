using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;

namespace osu.Game.Overlays.Pause
{
    public class PauseButton : Button
    {
        private float height = 100;
        private float width = 300;
        private float expandedWidth = 350;

        private AudioSample sampleClick;
        private AudioSample sampleHover;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            // Placeholder till the actual samples are added to osu-resources
            sampleClick = audio.Sample.Get(@"Menu/menuhit");
            sampleHover = audio.Sample.Get(@"Menu/menuclick");
        }

        protected override bool OnMouseDown(Framework.Input.InputState state, Framework.Graphics.MouseDownEventArgs args)
        {
            sampleClick.Play();

            return true;
        }

        protected override bool OnHover(Framework.Input.InputState state)
        {
            sampleHover.Play();
            ResizeTo(new Vector2(expandedWidth, height), 500, EasingTypes.OutElastic);

            return true;
        }

        protected override void OnHoverLost(Framework.Input.InputState state)
        {
            ResizeTo(new Vector2(width, height), 500, EasingTypes.OutElastic);
        }

        public PauseButton()
        {            Size = new Vector2(width, height);
            Colour = Color4.Black;
            Shear = new Vector2(0.1f, 0);
        }
    }
}
