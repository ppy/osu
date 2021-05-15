using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace Mvis.Plugin.RulesetPanel.Objects.Helpers
{
    public class CurrentBeatmapProvider : Container
    {
        protected IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        [BackgroundDependencyLoader]
        private void load(RulesetPanel panel)
        {
            Beatmap.BindTo(panel.CurrentBeatmap);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Beatmap.BindValueChanged(OnBeatmapChanged, true);
        }

        protected virtual void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
        }
    }
}
