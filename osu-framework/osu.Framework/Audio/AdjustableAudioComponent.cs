//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Cached;
using osu.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.Audio
{
    public class AdjustableAudioComponent : IDisposable, IUpdateable
    {
        private List<BindableDouble> volumeAdjustments = new List<BindableDouble>();
        private List<BindableDouble> balanceAdjustments = new List<BindableDouble>();
        private List<BindableDouble> frequencyAdjustments = new List<BindableDouble>();

        /// <summary>
        /// Global volume of this component.
        /// </summary>
        public readonly BindableDouble Volume = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        protected readonly BindableDouble VolumeCalculated = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// Playback balance of this sample (-1 .. 1 where 0 is centered)
        /// </summary>
        public readonly BindableDouble Balance = new BindableDouble(0) { MinValue = -1, MaxValue = 1 };

        protected readonly BindableDouble BalanceCalculated = new BindableDouble(0) { MinValue = -1, MaxValue = 1 };

        /// <summary>
        /// Rate at which the component is played back (affects pitch). 1 is 100% playback speed, or default frequency.
        /// </summary>
        public readonly BindableDouble Frequency = new BindableDouble(1);

        protected readonly BindableDouble FrequencyCalculated = new BindableDouble(1);

        /// <summary>
        /// Handles invalidation of the component's state.
        /// </summary>
        private Cached<double> componentState = new Cached<double>();

        protected AdjustableAudioComponent()
        {
            Volume.ValueChanged += InvalidateState;
            Balance.ValueChanged += InvalidateState;
            Frequency.ValueChanged += InvalidateState;
        }

        protected void InvalidateState(object sender = null, EventArgs e = null)
        {
            componentState.Invalidate();
        }

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            VolumeCalculated.Value = volumeAdjustments.Aggregate(Volume.Value, (current, adj) => current * adj);
            BalanceCalculated.Value = balanceAdjustments.Aggregate(Balance.Value, (current, adj) => current + adj);
            FrequencyCalculated.Value = frequencyAdjustments.Aggregate(Frequency.Value, (current, adj) => current * adj);
        }

        public void AddAdjustmentDependency(AdjustableAudioComponent component)
        {
            AddAdjustment(AdjustableProperty.Balance, component.BalanceCalculated);
            AddAdjustment(AdjustableProperty.Frequency, component.FrequencyCalculated);
            AddAdjustment(AdjustableProperty.Volume, component.VolumeCalculated);
        }

        public void RemoveAdjustmentDependency(AdjustableAudioComponent component)
        {
            RemoveAdjustment(component.BalanceCalculated);
            RemoveAdjustment(component.FrequencyCalculated);
            RemoveAdjustment(component.VolumeCalculated);
        }

        public void AddAdjustment(AdjustableProperty type, BindableDouble adjustBindable)
        {
            switch (type)
            {
                case AdjustableProperty.Balance:
                    balanceAdjustments.Add(adjustBindable);
                    break;
                case AdjustableProperty.Frequency:
                    frequencyAdjustments.Add(adjustBindable);
                    break;
                case AdjustableProperty.Volume:
                    volumeAdjustments.Add(adjustBindable);
                    break;

            }

            adjustBindable.ValueChanged += InvalidateState;

            InvalidateState();
        }

        public void RemoveAdjustment(BindableDouble adjustBindable)
        {
            balanceAdjustments.Remove(adjustBindable);
            frequencyAdjustments.Remove(adjustBindable);
            volumeAdjustments.Remove(adjustBindable);

            adjustBindable.ValueChanged -= InvalidateState;

            InvalidateState();
        }

        public virtual void Update()
        {
            componentState.Refresh(delegate
            {
                OnStateChanged(this, null);
                return 1;
            });
        }

        #region IDisposable Support
        protected bool IsDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    volumeAdjustments.ForEach(d => d.ValueChanged -= InvalidateState);
                    balanceAdjustments.ForEach(d => d.ValueChanged -= InvalidateState);
                    frequencyAdjustments.ForEach(d => d.ValueChanged -= InvalidateState);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public enum AdjustableProperty
    {
        Volume,
        Balance,
        Frequency
    }
}
