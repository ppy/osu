// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;

namespace osu.Mods.Evast.MusicVisualizers
{
    public abstract class MusicVisualizerBarsContainer : MusicVisualizerContainer
    {
        protected abstract VisualizerBar CreateNewBar();

        private int smoothness = 145;
        public int Smoothness
        {
            set
            {
                if (smoothness == value)
                    return;
                smoothness = value;

                if (!IsLoaded)
                    return;

                Restart();
            }
            get { return smoothness; }
        }

        private float barWidth = 4.5f;
        public float BarWidth
        {
            set
            {
                if (barWidth == value)
                    return;
                barWidth = value;

                if (!IsLoaded)
                    return;

                foreach (var bar in EqualizerBars)
                    bar.Width = barWidth;
            }
            get { return barWidth; }
        }

        private int barsAmount = 200;
        public int BarsAmount
        {
            set
            {
                if (barsAmount == value)
                    return;
                barsAmount = value;

                if (!IsLoaded)
                    return;

                Scheduler.CancelDelayedTasks();
                resetBars();
                Start();
            }
            get { return barsAmount; }
        }

        public float ValueMultiplier { get; set; } = 400;

        protected virtual void ClearBars()
        {
            if (Children.Count > 0)
                Clear(true);
        }

        private void resetBars()
        {
            ClearBars();
            rearrangeBars();
            AddBars();
        }

        private void rearrangeBars()
        {
            EqualizerBars = new VisualizerBar[barsAmount];
            for (int i = 0; i < barsAmount; i++)
            {
                EqualizerBars[i] = CreateNewBar();
                EqualizerBars[i].Width = barWidth;
            }
        }

        protected int RealAmplitudeFor(int barNumber) => 200 / BarsAmount * barNumber;

        protected VisualizerBar[] EqualizerBars;

        public bool IsReversed { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            rearrangeBars();
            AddBars();
        }

        protected virtual void AddBars()
        {
            foreach (var bar in EqualizerBars)
                Add(bar);
        }

        protected override void OnAmplitudesUpdate(float[] amplitudes)
        {
            for (int i = 0; i < BarsAmount; i++)
            {
                var currentAmplitude = amplitudes[RealAmplitudeFor(i)];
                EqualizerBars[IsReversed ? BarsAmount - 1 - i : i].SetValue(currentAmplitude, ValueMultiplier, Smoothness, UpdateDelay);
            }
        }

        protected abstract class VisualizerBar : Container
        {
            public abstract void SetValue(float amplitudeValue, float valueMultiplier, int softness, int faloff);
        }
    }
}
