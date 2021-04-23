using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace Mvis.Plugin.RulesetPanel.Objects.Helpers
{
    public class CurrentBeatmapProvider : Container
    {
        [Resolved]
        private RulesetPanel panel { get; set; }

        protected Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        [BackgroundDependencyLoader]
        private void load()
        {
            panel.OnMvisBeatmapChanged += onMvisBeatmapChanged;
            Beatmap.BindValueChanged(OnBeatmapChanged);
        }

        private void onMvisBeatmapChanged(WorkingBeatmap b) => Beatmap.Value = b;

        protected virtual void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
        }

        public virtual void StopResponseOnBeatmapChanges()
        {
            panel.OnMvisBeatmapChanged -= onMvisBeatmapChanged;
            Beatmap.Disabled = false;
        }

        public virtual void ResponseOnBeatmapChanges()
        {
            StopResponseOnBeatmapChanges();
            panel.OnMvisBeatmapChanged += onMvisBeatmapChanged;
        }
    }
}
