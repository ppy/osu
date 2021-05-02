using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Screens.Mvis;

namespace Mvis.Plugin.RulesetPanel.Objects.Helpers
{
    public class CurrentBeatmapProvider : Container
    {
        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        protected Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.BindValueChanged(OnBeatmapChanged);
        }

        protected virtual void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
        }

        public virtual void StopResponseOnBeatmapChanges()
        {
            Beatmap.UnbindFrom(mvisScreen.Beatmap);
            Beatmap.Disabled = false;
        }

        public virtual void ResponseOnBeatmapChanges()
        {
            StopResponseOnBeatmapChanges();

            Beatmap.BindTo(mvisScreen.Beatmap);
            Beatmap.TriggerChange();
        }
    }
}
