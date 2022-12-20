using Mvis.Plugin.SandboxToPanel.RulesetComponents.UI;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.UI.Settings;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Settings
{
    public partial class TrackSection : SandboxSettingsSection
    {
        protected override string HeaderName => "Track";

        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; }

        private readonly BindableBool loopCurrent = new BindableBool();

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(new Drawable[]
            {
                new TrackController(),
                new SettingsCheckbox
                {
                    LabelText = "Loop Current Track",
                    Current = loopCurrent
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            loopCurrent.BindValueChanged(loop => updateLooping(working.Value, loop.NewValue));
            working.BindValueChanged(w => updateLooping(w.NewValue, loopCurrent.Value), true);
        }

        private static void updateLooping(WorkingBeatmap beatmap, bool isLooping)
        {
            if (beatmap != null && beatmap.Track != null)
                beatmap.Track.Looping = isLooping;
        }

        protected override void Dispose(bool isDisposing)
        {
            updateLooping(working.Value, false);
            base.Dispose(isDisposing);
        }
    }
}
