using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace Mvis.Plugin.Sandbox.Components.MusicHelpers
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
