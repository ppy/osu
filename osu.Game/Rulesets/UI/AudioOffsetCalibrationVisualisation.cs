// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Audio;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A ruleset's gameplay cue and input handling for audio offset calibration.
    /// </summary>
    public abstract partial class AudioOffsetCalibrationVisualisation : CompositeDrawable
    {
        public Func<double> GetReferenceTime { get; set; } = () => 0;
        public double BeatLength { get; set; } = AudioOffsetCalibrationTrackStore.BEAT_LENGTH;
        public Action? Tapped { get; set; }

        protected readonly Container HitObjectContainer;

        private readonly ManualClock animationClock = new ManualClock();
        private double? previousBeat;
        private double previousBeatLength;

        protected AudioOffsetCalibrationVisualisation()
        {
            RelativeSizeAxes = Axes.Both;
            AddInternal(HitObjectContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Clock = new FramedClock(animationClock),
            });
        }

        protected override void Update()
        {
            base.Update();

            // Leave a short gap after the hit so its disappearance is easy to see.
            double referenceTime = GetReferenceTime();
            double beat = Math.Floor((referenceTime - AudioOffsetCalibrationTrackStore.FIRST_BEAT) / BeatLength + 0.75);
            double time = referenceTime - AudioOffsetCalibrationTrackStore.FIRST_BEAT - beat * BeatLength;
            animationClock.CurrentTime = time;

            if (beat != previousBeat || BeatLength != previousBeatLength)
            {
                previousBeat = beat;
                previousBeatLength = BeatLength;
                RecreateHitObject(((int)beat % 4 + 4) % 4);
            }

            // The reference track supplies the sound. Never play frame-scheduled hitsounds here.
            HitObjectContainer.Alpha = time < 0 ? 1 : 0;
            UpdateHitObject(time);
        }

        protected abstract void RecreateHitObject(int beat);

        protected virtual void UpdateHitObject(double time)
        {
        }
    }
}
