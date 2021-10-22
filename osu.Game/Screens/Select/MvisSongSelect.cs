using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Overlays;

namespace osu.Game.Screens.Select
{
    public class LLinSongSelect : SongSelect
    {
        public override bool HideOverlaysOnEnter => true;

        [Resolved]
        private MusicController musicController { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            musicController.CurrentTrack.Looping = true;
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MvisBeatmapDetailArea
        {
            SelectCurrentAction = () => this.OnStart(),
        };

        public override bool AllowEditing => false;

        protected override bool OnStart()
        {
            SampleConfirm?.Play();

            if ( BeatmapSetChanged )
                musicController.SeekTo(0);

            this.Exit();

            return true;
        }
    }
}
