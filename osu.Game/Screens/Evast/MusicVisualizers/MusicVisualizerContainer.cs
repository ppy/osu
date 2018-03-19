// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Evast.MusicVisualizers
{
    public abstract class MusicVisualizerContainer : Container
    {
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        protected abstract VisualizerBar CreateNewBar();

        private int updateDelay = 1;
        public int UpdateDelay
        {
            set
            {
                if (updateDelay == value)
                    return;
                updateDelay = value;

                if (!IsLoaded)
                    return;

                restart();
            }
            get { return updateDelay; }
        }

        private int softness = 100;
        public int Softness
        {
            set
            {
                if (softness == value)
                    return;
                softness = value;

                if (!IsLoaded)
                    return;

                restart();
            }
            get { return softness; }
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
                updateAmplitudes();
            }
            get { return barsAmount; }
        }

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

        public float ValueMultiplier { get; set; } = 400;

        protected int RealAmplitudeFor(int barNumber) => 200 / BarsAmount * barNumber;

        protected VisualizerBar[] EqualizerBars;

        public bool IsReversed { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap.BindTo(game.Beatmap);
            rearrangeBars();
            AddBars();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateAmplitudes();
        }

        protected virtual void AddBars()
        {
            foreach (var bar in EqualizerBars)
                Add(bar);
        }

        private void updateAmplitudes()
        {
            var frequencyAmplitudes = beatmap.Value.Track?.CurrentAmplitudes.FrequencyAmplitudes ?? new float[256];

            for (int i = 0; i < BarsAmount; i++)
            {
                var currentAmplitude = frequencyAmplitudes[RealAmplitudeFor(i)];
                EqualizerBars[IsReversed ? BarsAmount - 1 - i : i].SetValue(currentAmplitude, ValueMultiplier, Softness, UpdateDelay);
            }

            Scheduler.AddDelayed(updateAmplitudes, UpdateDelay);
        }

        private void restart()
        {
            Scheduler.CancelDelayedTasks();
            updateAmplitudes();
        }

        protected abstract class VisualizerBar : Container
        {
            public abstract void SetValue(float amplitudeValue, float valueMultiplier, int softness, int faloff);
        }
    }
}
