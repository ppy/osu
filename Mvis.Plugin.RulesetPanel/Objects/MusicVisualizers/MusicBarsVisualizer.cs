using System;
using Mvis.Plugin.RulesetPanel.Config;
using Mvis.Plugin.RulesetPanel.Objects.Helpers;
using Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers.Bars;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers
{
    public abstract class MusicBarsVisualizer : MusicAmplitudesProvider
    {
        [Resolved]
        private RulesetPanelConfigManager config { get; set; }

        private readonly Bindable<MvisBarType> type = new Bindable<MvisBarType>(MvisBarType.Rounded);
        private readonly Bindable<double> barWidthBindable = new Bindable<double>(3.0);

        public int Smoothness = 200;

        private float barWidth = 4.5f;

        public float BarWidth
        {
            get => barWidth;
            set
            {
                barWidth = value;

                if (!IsLoaded)
                    return;

                if (type.Value == MvisBarType.Rounded)
                {
                    foreach (var bar in EqualizerBars)
                        ((CircularBar)bar).Width = value;

                    return;
                }

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

        public float ValueMultiplier = 400;

        [BackgroundDependencyLoader]
        private void load()
        {
            config.BindWith(RulesetPanelSetting.BarType, type);
            config.BindWith(RulesetPanelSetting.BarWidth, barWidthBindable);

            barWidthBindable.BindValueChanged(width => BarWidth = (float)width.NewValue);
            type.BindValueChanged(_ => resetBars(), true);
        }

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
                EqualizerBars[i] = getBar();
                EqualizerBars[i].Width = BarWidth;
            }
        }

        private BasicBar getBar()
        {
            switch (type.Value)
            {
                case MvisBarType.Basic:
                    return new BasicBar();

                case MvisBarType.Rounded:
                    return new CircularBar();

                case MvisBarType.Fall:
                    return new FallBar();
            }

            throw new NotSupportedException("Selected bar is not implemented");
        }

        protected BasicBar[] EqualizerBars;

        public bool IsReversed { get; set; }

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
