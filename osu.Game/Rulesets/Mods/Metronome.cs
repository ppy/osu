// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mods
{
    public class Metronome : BeatSyncedContainer
    {
        private readonly double firstHitTime;

        private PausableSkinnableSound sample;

        /// <param name="firstHitTime">Start time of the first hit object, used for providing a count down.</param>
        public Metronome(double firstHitTime)
        {
            this.firstHitTime = firstHitTime;
            AllowMistimedEventFiring = false;
            Divisor = 1;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                sample = new PausableSkinnableSound(new SampleInfo("Gameplay/catch-banana"))
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (!IsBeatSyncedWithTrack) return;

            int timeSignature = (int)timingPoint.TimeSignature;

            // play metronome from one measure before the first object.
            if (BeatSyncClock.CurrentTime < firstHitTime - timingPoint.BeatLength * timeSignature)
                return;

            sample.Frequency.Value = beatIndex % timeSignature == 0 ? 1 : 0.5f;
            sample.Play();
        }
    }
}
