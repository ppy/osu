using osu.Framework.Graphics.Audio;

namespace osu.Game.Screens.Mvis.Plugins.Types
{
    public interface IProvideAudioControlPlugin
    {
        public void NextTrack();

        public void PrevTrack();

        public void TogglePause();

        public void Seek(double position);

        public DrawableTrack GetCurrentTrack();

        bool IsCurrent { get; set; }
    }
}
