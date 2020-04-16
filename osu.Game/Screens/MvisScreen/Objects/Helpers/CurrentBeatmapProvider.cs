using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Mvis.UI.Objects.Helpers
{
    public class CurrentBeatmapProvider : Container
    {
        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; }

        protected Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Beatmap.BindTo(working);
            Beatmap.BindValueChanged(OnBeatmapChanged, true);
        }

        protected virtual void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
        }
    }
}
