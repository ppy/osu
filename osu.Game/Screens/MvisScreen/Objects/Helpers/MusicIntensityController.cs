using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Screens.Mvis.UI.Objects.Helpers
{
    public class MusicIntensityController : MusicAmplitudesProvider
    {
        public readonly BindableFloat Intensity = new BindableFloat();

        protected override void OnAmplitudesUpdate(float[] amplitudes)
        {
            float sum = 0;
            amplitudes.ForEach(amp => sum += amp);

            if (IsKiai.Value)
                sum *= 1.2f;

            Intensity.Value = sum;
        }
    }
}
