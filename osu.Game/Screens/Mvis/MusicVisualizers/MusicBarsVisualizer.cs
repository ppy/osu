using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Screens.Mvis.UI.Objects.Helpers;
using osu.Game.Screens.Mvis.UI.Objects.MusicVisualizers.Bars;

namespace osu.Game.Screens.Mvis.UI.Objects.MusicVisualizers
{
    public abstract class MusicBarsVisualizer : MusicAmplitudesProvider
    {
        protected virtual BasicBar CreateBar() => new BasicBar();

        public int Smoothness { get; set; } = 200;

        private float barWidth = 4.5f;
        public float BarWidth
        {
            get => barWidth;
            set
            {
                barWidth = value;

                if (!IsLoaded)
                    return;

                foreach (var bar in EqualizerBars)
                    bar.Width = value;
            }
        }

        private int barsCount = 200;
        public int BarsCount
        {
            get => barsCount;
            set
            {
                barsCount = value;

                if (!IsLoaded)
                    return;

                resetBars();
            }
        }

        public float ValueMultiplier { get; set; } = 400;

        protected virtual void ClearBars() => Clear(true);

        private void resetBars()
        {
            ClearBars();
            rearrangeBars();
            AddBars();
        }

        private void rearrangeBars()
        {
            EqualizerBars = new BasicBar[barsCount];
            for (int i = 0; i < barsCount; i++)
            {
                EqualizerBars[i] = CreateBar();
                EqualizerBars[i].Width = barWidth;
            }
        }

        protected BasicBar[] EqualizerBars;

        public bool IsReversed { get; set; }

        protected override void LoadComplete()
        {
            resetBars();
            base.LoadComplete();
        }

        protected virtual void AddBars() => EqualizerBars.ForEach(Add);

        protected override void OnAmplitudesUpdate(float[] amplitudes)
        {
            var amps = new float[barsCount];

            for (int i = 0; i < barsCount; i++)
            {
                if (i == 0)
                {
                    amps[i] = amplitudes[getAmpIndexForBar(i)];
                    continue;
                }

                var nextAmp = i == barsCount - 1 ? 0 : amplitudes[getAmpIndexForBar(i + 1)];

                amps[i] = (amps[i - 1] + amplitudes[getAmpIndexForBar(i)] + nextAmp) / 3f;
            }

            for (int i = 0; i < barsCount; i++)
            {
                EqualizerBars[IsReversed ? barsCount - 1 - i : i].SetValue(amps[i], ValueMultiplier, Smoothness);
            }
        }

        private int getAmpIndexForBar(int barIndex) => 200 / barsCount * barIndex;
    }
}
