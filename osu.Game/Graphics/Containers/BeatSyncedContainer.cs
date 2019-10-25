// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Graphics.Containers
{
    public class BeatSyncedContainer : Container
    {
        protected readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private int lastBeat;
        private TimingControlPoint lastTimingPoint;

        /// <summary>
        /// The amount of time before a beat we should fire <see cref="OnNewBeat(int, TimingControlPoint, EffectControlPoint, TrackAmplitudes)"/>.
        /// This allows for adding easing to animations that may be synchronised to the beat.
        /// </summary>
        protected double EarlyActivationMilliseconds;

        /// <summary>
        /// The time in milliseconds until the next beat.
        /// </summary>
        public double TimeUntilNextBeat { get; private set; }

        /// <summary>
        /// The time in milliseconds since the last beat
        /// </summary>
        public double TimeSinceLastBeat { get; private set; }

        /// <summary>
        /// Default length of a beat in milliseconds. Used whenever there is no beatmap or track playing.
        /// </summary>
        private const double default_beat_length = 60000.0 / 60.0;

        private TimingControlPoint defaultTiming;
        private EffectControlPoint defaultEffect;
        private TrackAmplitudes defaultAmplitudes;

        protected override void Update()
        {
            Track track = null;
            IBeatmap beatmap = null;

            double currentTrackTime;
            TimingControlPoint timingPoint;
            EffectControlPoint effectPoint;

            if (Beatmap.Value.TrackLoaded && Beatmap.Value.BeatmapLoaded)
            {
                track = Beatmap.Value.Track;
                beatmap = Beatmap.Value.Beatmap;
            }

            if (track != null && beatmap != null && track.IsRunning)
            {
                currentTrackTime = track.Length > 0 ? track.CurrentTime + EarlyActivationMilliseconds : Clock.CurrentTime;

                timingPoint = beatmap.ControlPointInfo.TimingPointAt(currentTrackTime);
                effectPoint = beatmap.ControlPointInfo.EffectPointAt(currentTrackTime);

                if (timingPoint.BeatLength == 0)
                    return;
            }
            else
            {
                currentTrackTime = Clock.CurrentTime;
                timingPoint = defaultTiming;
                effectPoint = defaultEffect;
            }

            int beatIndex = (int)((currentTrackTime - timingPoint.Time) / timingPoint.BeatLength);

            // The beats before the start of the first control point are off by 1, this should do the trick
            if (currentTrackTime < timingPoint.Time)
                beatIndex--;

            TimeUntilNextBeat = (timingPoint.Time - currentTrackTime) % timingPoint.BeatLength;
            if (TimeUntilNextBeat < 0)
                TimeUntilNextBeat += timingPoint.BeatLength;

            TimeSinceLastBeat = timingPoint.BeatLength - TimeUntilNextBeat;

            if (timingPoint.Equals(lastTimingPoint) && beatIndex == lastBeat)
                return;

            using (BeginDelayedSequence(-TimeSinceLastBeat, true))
                OnNewBeat(beatIndex, timingPoint, effectPoint, track?.CurrentAmplitudes ?? defaultAmplitudes);

            lastBeat = beatIndex;
            lastTimingPoint = timingPoint;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            Beatmap.BindTo(beatmap);

            defaultTiming = new TimingControlPoint
            {
                BeatLength = default_beat_length,
            };

            defaultEffect = new EffectControlPoint
            {
                KiaiMode = false,
                OmitFirstBarLine = false
            };

            defaultAmplitudes = new TrackAmplitudes
            {
                FrequencyAmplitudes = new float[256],
                LeftChannel = 0,
                RightChannel = 0
            };
        }

        protected virtual void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
        }
    }
}
