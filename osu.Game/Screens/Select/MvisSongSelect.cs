using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Overlays;

namespace osu.Game.Screens.Select
{
    public class MvisSongSelect : SongSelect
    {
        public override bool HideOverlaysOnEnter => true;

        [Resolved]
        private MusicController musicController { get; set; }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MvisBeatmapDetailArea
        {
            OnPressedAction = () => this.OnStart()
        };

        public override bool AllowEditing => false;

        protected override bool OnStart()
        {
            SampleConfirm?.Play();
            musicController.SeekTo(0);

            this.Exit();

            return true;
        }
    }
}