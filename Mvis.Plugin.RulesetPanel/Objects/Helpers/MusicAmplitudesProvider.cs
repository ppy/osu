using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Overlays;

namespace Mvis.Plugin.RulesetPanel.Objects.Helpers
{
    public abstract class MusicAmplitudesProvider : CurrentBeatmapProvider
    {
        public readonly BindableBool IsKiai = new BindableBool();

        [Resolved]
        private MusicController controller { get; set; }

        protected override void Update()
        {
            base.Update();

            var track = controller.CurrentTrack;
            OnAmplitudesUpdate(track.CurrentAmplitudes.FrequencyAmplitudes.Span.ToArray() ?? new float[256]);
            IsKiai.Value = Beatmap.Value?.Beatmap.ControlPointInfo.EffectPointAt(track.CurrentTime).KiaiMode ?? false;
        }

        protected abstract void OnAmplitudesUpdate(float[] amplitudes);
    }
}
