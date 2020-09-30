using osu.Framework.Screens;

namespace osu.Game.Screens.Select
{
    public class MvisSongSelect : SongSelect
    {
        public override bool HideOverlaysOnEnter => true;

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MvisBeatmapDetailArea
        {
            OnPressedAction = () => this.OnStart()
        };

        public override bool AllowEditing => false;

        protected override bool OnStart()
        {
            this.Exit();

            return true;
        }
    }
}