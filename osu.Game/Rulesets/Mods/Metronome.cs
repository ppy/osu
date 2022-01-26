// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mods
{
    public class Metronome : BeatSyncedContainer, IAdjustableAudioComponent
    {
        private readonly double firstHitTime;

        private readonly PausableSkinnableSound sample;

        /// <param name="firstHitTime">Start time of the first hit object, used for providing a count down.</param>
        public Metronome(double firstHitTime)
        {
            this.firstHitTime = firstHitTime;
            AllowMistimedEventFiring = false;
            Divisor = 1;

            InternalChild = sample = new PausableSkinnableSound(new SampleInfo("Gameplay/catch-banana"));
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (!IsBeatSyncedWithTrack) return;

            int timeSignature = timingPoint.TimeSignature.Numerator;

            // play metronome from one measure before the first object.
            if (BeatSyncClock.CurrentTime < firstHitTime - timingPoint.BeatLength * timeSignature)
                return;

            sample.Frequency.Value = beatIndex % timeSignature == 0 ? 1 : 0.5f;
            sample.Play();
        }

        #region IAdjustableAudioComponent

        public IBindable<double> AggregateVolume => sample.AggregateVolume;

        public IBindable<double> AggregateBalance => sample.AggregateBalance;

        public IBindable<double> AggregateFrequency => sample.AggregateFrequency;

        public IBindable<double> AggregateTempo => sample.AggregateTempo;

        public BindableNumber<double> Volume => sample.Volume;

        public BindableNumber<double> Balance => sample.Balance;

        public BindableNumber<double> Frequency => sample.Frequency;

        public BindableNumber<double> Tempo => sample.Tempo;

        public void BindAdjustments(IAggregateAudioAdjustment component)
        {
            sample.BindAdjustments(component);
        }

        public void UnbindAdjustments(IAggregateAudioAdjustment component)
        {
            sample.UnbindAdjustments(component);
        }

        public void AddAdjustment(AdjustableProperty type, IBindable<double> adjustBindable)
        {
            sample.AddAdjustment(type, adjustBindable);
        }

        public void RemoveAdjustment(AdjustableProperty type, IBindable<double> adjustBindable)
        {
            sample.RemoveAdjustment(type, adjustBindable);
        }

        public void RemoveAllAdjustments(AdjustableProperty type)
        {
            sample.RemoveAllAdjustments(type);
        }

        #endregion
    }
}
