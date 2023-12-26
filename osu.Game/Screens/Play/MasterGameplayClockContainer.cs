// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A <see cref="GameplayClockContainer"/> which uses a <see cref="WorkingBeatmap"/> as a source.
    /// <para>
    /// This is the most complete <see cref="GameplayClockContainer"/> which takes into account all user and platform offsets,
    /// and provides implementations for user actions such as skipping or adjusting playback rates that may occur during gameplay.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This is intended to be used as a single controller for gameplay, or as a reference source for other <see cref="GameplayClockContainer"/>s.
    /// </remarks>
    public partial class MasterGameplayClockContainer : GameplayClockContainer, IBeatSyncProvider
    {
        /// <summary>
        /// Duration before gameplay start time required before skip button displays.
        /// </summary>
        public const double MINIMUM_SKIP_TIME = 1000;

        public readonly BindableNumber<double> UserPlaybackRate = new BindableDouble(1)
        {
            MinValue = 0.5,
            MaxValue = 2,
            Precision = 0.1,
        };

        /// <summary>
        /// Whether the audio playback is within acceptable ranges.
        /// Will become false if audio playback is not going as expected.
        /// </summary>
        public IBindable<bool> PlaybackRateValid => playbackRateValid;

        private readonly Bindable<bool> playbackRateValid = new Bindable<bool>(true);

        private readonly WorkingBeatmap beatmap;

        private Track track;

        private readonly double skipTargetTime;

        /// <summary>
        /// Stores the time at which the last <see cref="StopGameplayClock"/> call was triggered.
        /// This is used to ensure we resume from that precise point in time, ignoring the proceeding frequency ramp.
        ///
        /// Optimally, we'd have gameplay ramp down with the frequency, but I believe this was intentionally disabled
        /// to avoid fails occurring after the pause screen has been shown.
        ///
        /// In the future I want to change this.
        /// </summary>
        internal double? LastStopTime;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        /// <summary>
        /// Create a new master gameplay clock container.
        /// </summary>
        /// <param name="beatmap">The beatmap to be used for time and metadata references.</param>
        /// <param name="skipTargetTime">The latest time which should be used when introducing gameplay. Will be used when skipping forward.</param>
        public MasterGameplayClockContainer(WorkingBeatmap beatmap, double skipTargetTime)
            : base(beatmap.Track, applyOffsets: true, requireDecoupling: true)
        {
            this.beatmap = beatmap;
            this.skipTargetTime = skipTargetTime;

            track = beatmap.Track;

            StartTime = findEarliestStartTime();
        }

        private double findEarliestStartTime()
        {
            // here we are trying to find the time to start playback from the "zero" point.
            // generally this is either zero, or some point earlier than zero in the case of storyboards, lead-ins etc.

            // start with the originally provided latest time (if before zero).
            double time = Math.Min(0, skipTargetTime);

            // if a storyboard is present, it may dictate the appropriate start time by having events in negative time space.
            // this is commonly used to display an intro before the audio track start.
            double? firstStoryboardEvent = beatmap.Storyboard.EarliestEventTime;
            if (firstStoryboardEvent != null)
                time = Math.Min(time, firstStoryboardEvent.Value);

            // some beatmaps specify a current lead-in time which should be used instead of the ruleset-provided value when available.
            // this is not available as an option in the live editor but can still be applied via .osu editing.
            double firstHitObjectTime = beatmap.Beatmap.HitObjects.First().StartTime;
            if (beatmap.BeatmapInfo.AudioLeadIn > 0)
                time = Math.Min(time, firstHitObjectTime - beatmap.BeatmapInfo.AudioLeadIn);

            return time;
        }

        protected override void StopGameplayClock()
        {
            LastStopTime = GameplayClock.CurrentTime;

            if (IsLoaded)
            {
                // During normal operation, the source is stopped after performing a frequency ramp.
                this.TransformBindableTo(GameplayClock.ExternalPauseFrequencyAdjust, 0, 200, Easing.Out).OnComplete(_ =>
                {
                    if (IsPaused.Value)
                        base.StopGameplayClock();
                });
            }
            else
            {
                base.StopGameplayClock();

                // If not yet loaded, we still want to ensure relevant state is correct, as it is used for offset calculations.
                GameplayClock.ExternalPauseFrequencyAdjust.Value = 0;

                // We must also process underlying gameplay clocks to update rate-adjusted offsets with the new frequency adjustment.
                // Without doing this, an initial seek may be performed with the wrong offset.
                GameplayClock.ProcessFrame();
            }
        }

        public override void Seek(double time)
        {
            // Safety in case the clock is seeked while stopped.
            LastStopTime = null;
            elapsedValidationTime = null;

            base.Seek(time);
        }

        protected override void PrepareStart()
        {
            if (LastStopTime != null)
            {
                Seek(LastStopTime.Value);
                LastStopTime = null;
            }
            else
                base.PrepareStart();
        }

        protected override void StartGameplayClock()
        {
            addAdjustmentsToTrack();

            base.StartGameplayClock();

            if (IsLoaded)
            {
                this.TransformBindableTo(GameplayClock.ExternalPauseFrequencyAdjust, 1, 200, Easing.In);
            }
            else
            {
                // If not yet loaded, we still want to ensure relevant state is correct, as it is used for offset calculations.
                GameplayClock.ExternalPauseFrequencyAdjust.Value = 1;

                // We must also process underlying gameplay clocks to update rate-adjusted offsets with the new frequency adjustment.
                // Without doing this, an initial seek may be performed with the wrong offset.
                GameplayClock.ProcessFrame();
            }
        }

        /// <summary>
        /// Skip forward to the next valid skip point.
        /// </summary>
        public void Skip()
        {
            if (GameplayClock.CurrentTime > skipTargetTime - MINIMUM_SKIP_TIME)
                return;

            double skipTarget = skipTargetTime - MINIMUM_SKIP_TIME;

            if (GameplayClock.CurrentTime < 0 && skipTarget > 6000)
                // double skip exception for storyboards with very long intros
                skipTarget = 0;

            Seek(skipTarget);
        }

        /// <summary>
        /// Changes the backing clock to avoid using the originally provided track.
        /// </summary>
        public void StopUsingBeatmapClock()
        {
            removeAdjustmentsFromTrack();

            track = new TrackVirtual(beatmap.Track.Length);
            track.Seek(CurrentTime);
            if (IsRunning)
                track.Start();
            ChangeSource(track);

            addAdjustmentsToTrack();
        }

        protected override void Update()
        {
            base.Update();
            checkPlaybackValidity();
        }

        #region Clock validation (ensure things are running correctly for local gameplay)

        private double elapsedGameplayClockTime;
        private double? elapsedValidationTime;
        private int playbackDiscrepancyCount;

        private const int allowed_playback_discrepancies = 5;

        private void checkPlaybackValidity()
        {
            if (GameplayClock.IsRunning)
            {
                elapsedGameplayClockTime += GameplayClock.ElapsedFrameTime;

                if (elapsedValidationTime == null)
                    elapsedValidationTime = elapsedGameplayClockTime;
                else
                    elapsedValidationTime += GameplayClock.Rate * Time.Elapsed;

                if (Math.Abs(elapsedGameplayClockTime - elapsedValidationTime!.Value) > 300)
                {
                    if (playbackDiscrepancyCount++ > allowed_playback_discrepancies)
                    {
                        if (playbackRateValid.Value)
                        {
                            playbackRateValid.Value = false;
                            Logger.Log("System audio playback is not working as expected. Some online functionality will not work.\n\nPlease check your audio drivers.", level: LogLevel.Important);
                        }
                    }
                    else
                    {
                        Logger.Log($"Playback discrepancy detected ({playbackDiscrepancyCount} of allowed {allowed_playback_discrepancies}): {elapsedGameplayClockTime:N1} vs {elapsedValidationTime:N1}");
                    }

                    elapsedValidationTime = null;
                }
            }
        }

        #endregion

        private bool speedAdjustmentsApplied;

        private void addAdjustmentsToTrack()
        {
            if (speedAdjustmentsApplied)
                return;

            musicController.ResetTrackAdjustments();

            track.BindAdjustments(AdjustmentsFromMods);
            track.AddAdjustment(AdjustableProperty.Frequency, GameplayClock.ExternalPauseFrequencyAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            speedAdjustmentsApplied = true;
        }

        private void removeAdjustmentsFromTrack()
        {
            if (!speedAdjustmentsApplied)
                return;

            track.UnbindAdjustments(AdjustmentsFromMods);
            track.RemoveAdjustment(AdjustableProperty.Frequency, GameplayClock.ExternalPauseFrequencyAdjust);
            track.RemoveAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            speedAdjustmentsApplied = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            removeAdjustmentsFromTrack();
        }

        ControlPointInfo IBeatSyncProvider.ControlPoints => beatmap.Beatmap.ControlPointInfo;
        IClock IBeatSyncProvider.Clock => this;

        ChannelAmplitudes IHasAmplitudes.CurrentAmplitudes => beatmap.TrackLoaded ? beatmap.Track.CurrentAmplitudes : ChannelAmplitudes.Empty;
    }
}
