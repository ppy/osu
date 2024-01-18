// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Input.Handlers;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A container which consumes a parent gameplay clock and standardises frame counts for children.
    /// Will ensure a minimum of 50 frames per clock second is maintained, regardless of any system lag or seeks.
    /// </summary>
    [Cached(typeof(IGameplayClock))]
    [Cached(typeof(IFrameStableClock))]
    public sealed partial class FrameStabilityContainer : Container, IHasReplayHandler, IFrameStableClock
    {
        public ReplayInputHandler? ReplayInputHandler { get; set; }

        /// <summary>
        /// The number of frames (per parent frame) which can be run in an attempt to catch-up to real-time.
        /// </summary>
        public int MaxCatchUpFrames { get; set; } = 5;

        /// <summary>
        /// Whether to enable frame-stable playback.
        /// </summary>
        internal bool FrameStablePlayback { get; set; } = true;

        private readonly Bindable<bool> isCatchingUp = new Bindable<bool>();

        private readonly Bindable<bool> waitingOnFrames = new Bindable<bool>();

        private readonly double gameplayStartTime;

        private IGameplayClock? parentGameplayClock;

        /// <summary>
        /// A clock which is used as reference for time, rate and running state.
        /// </summary>
        private IClock referenceClock = null!;

        /// <summary>
        /// A local manual clock which tracks the reference clock.
        /// Values are transferred from <see cref="referenceClock"/> each update call.
        /// </summary>
        private readonly ManualClock manualClock;

        /// <summary>
        /// The main framed clock which has stability applied to it.
        /// This gets exposed to children as an <see cref="IGameplayClock"/>.
        /// </summary>
        private readonly FramedClock framedClock;

        /// <summary>
        /// The current direction of playback to be exposed to frame stable children.
        /// </summary>
        /// <remarks>
        /// Initially it is presumed that playback will proceed in the forward direction.
        /// </remarks>
        private int direction = 1;

        private PlaybackState state;

        private bool hasReplayAttached => ReplayInputHandler != null;

        private bool firstConsumption = true;

        public FrameStabilityContainer(double gameplayStartTime = double.MinValue)
        {
            RelativeSizeAxes = Axes.Both;

            framedClock = new FramedClock(manualClock = new ManualClock());

            this.gameplayStartTime = gameplayStartTime;
        }

        [BackgroundDependencyLoader(true)]
        private void load(IGameplayClock? gameplayClock)
        {
            if (gameplayClock != null)
            {
                parentGameplayClock = gameplayClock;
                IsPaused.BindTo(parentGameplayClock.IsPaused);
            }

            referenceClock = gameplayClock ?? Clock;
            Clock = this;
        }

        public override bool UpdateSubTree()
        {
            int loops = MaxCatchUpFrames;

            do
            {
                // update clock is always trying to approach the aim time.
                // it should be provided as the original value each loop.
                updateClock();

                if (state == PlaybackState.NotValid)
                    break;

                base.UpdateSubTree();
                UpdateSubTreeMasking(this, ScreenSpaceDrawQuad.AABBFloat);
            } while (state == PlaybackState.RequiresCatchUp && loops-- > 0);

            return true;
        }

        private void updateClock()
        {
            if (waitingOnFrames.Value)
            {
                // if waiting on frames, run one update loop to determine if frames have arrived.
                state = PlaybackState.Valid;
            }
            else if (IsPaused.Value && !hasReplayAttached)
            {
                // time should not advance while paused, nor should anything run.
                state = PlaybackState.NotValid;
                return;
            }
            else
            {
                state = PlaybackState.Valid;
            }

            double proposedTime = referenceClock.CurrentTime;

            if (FrameStablePlayback)
                // if we require frame stability, the proposed time will be adjusted to move at most one known
                // frame interval in the current direction.
                applyFrameStability(ref proposedTime);

            if (hasReplayAttached)
            {
                bool valid = updateReplay(ref proposedTime);

                if (!valid)
                    state = PlaybackState.NotValid;
            }

            // if the proposed time is the same as the current time, assume that the clock will continue progressing in the same direction as previously.
            // this avoids spurious flips in direction from -1 to 1 during rewinds.
            if (state == PlaybackState.Valid && proposedTime != manualClock.CurrentTime)
                direction = proposedTime >= manualClock.CurrentTime ? 1 : -1;

            double timeBehind = Math.Abs(proposedTime - referenceClock.CurrentTime);

            isCatchingUp.Value = timeBehind > 200;
            waitingOnFrames.Value = state == PlaybackState.NotValid;

            manualClock.CurrentTime = proposedTime;
            manualClock.Rate = Math.Abs(referenceClock.Rate) * direction;
            manualClock.IsRunning = referenceClock.IsRunning;

            // determine whether catch-up is required.
            if (state == PlaybackState.Valid && timeBehind > 0)
                state = PlaybackState.RequiresCatchUp;

            // The manual clock time has changed in the above code. The framed clock now needs to be updated
            // to ensure that the its time is valid for our children before input is processed
            framedClock.ProcessFrame();

            if (framedClock.ElapsedFrameTime != 0)
                IsRewinding = framedClock.ElapsedFrameTime < 0;
        }

        /// <summary>
        /// Attempt to advance replay playback for a given time.
        /// </summary>
        /// <param name="proposedTime">The time which is to be displayed.</param>
        /// <returns>Whether playback is still valid.</returns>
        private bool updateReplay(ref double proposedTime)
        {
            Debug.Assert(ReplayInputHandler != null);

            double? newTime;

            if (FrameStablePlayback)
            {
                // when stability is turned on, we shouldn't execute for time values the replay is unable to satisfy.
                newTime = ReplayInputHandler.SetFrameFromTime(proposedTime);
            }
            else
            {
                // when stability is disabled, we don't really care about accuracy.
                // looping over the replay will allow it to catch up and feed out the required values
                // for the current time.
                while ((newTime = ReplayInputHandler.SetFrameFromTime(proposedTime)) != proposedTime)
                {
                    if (newTime == null)
                    {
                        // special case for when the replay actually can't arrive at the required time.
                        // protects from potential endless loop.
                        break;
                    }
                }
            }

            if (newTime == null)
                return false;

            proposedTime = newTime.Value;
            return true;
        }

        /// <summary>
        /// Apply frame stability modifier to a time.
        /// </summary>
        /// <param name="proposedTime">The time which is to be displayed.</param>
        private void applyFrameStability(ref double proposedTime)
        {
            const double sixty_frame_time = 1000.0 / 60;

            if (firstConsumption)
            {
                // On the first update, frame-stability seeking would result in unexpected/unwanted behaviour.
                // Instead we perform an initial seek to the proposed time.

                // process frame (in addition to finally clause) to clear out ElapsedTime
                manualClock.CurrentTime = proposedTime;
                framedClock.ProcessFrame();

                firstConsumption = false;
                return;
            }

            if (manualClock.CurrentTime < gameplayStartTime)
                manualClock.CurrentTime = proposedTime = Math.Min(gameplayStartTime, proposedTime);
            else if (Math.Abs(manualClock.CurrentTime - proposedTime) > sixty_frame_time * 1.2f)
            {
                proposedTime = proposedTime > manualClock.CurrentTime
                    ? Math.Min(proposedTime, manualClock.CurrentTime + sixty_frame_time)
                    : Math.Max(proposedTime, manualClock.CurrentTime - sixty_frame_time);
            }
        }

        #region Delegation of IGameplayClock

        public IBindable<bool> IsPaused { get; } = new BindableBool();

        public bool IsRewinding { get; private set; }

        public double CurrentTime => framedClock.CurrentTime;

        public double Rate => framedClock.Rate;

        public bool IsRunning => framedClock.IsRunning;

        public void ProcessFrame() { }

        public double ElapsedFrameTime => framedClock.ElapsedFrameTime;

        public double FramesPerSecond => framedClock.FramesPerSecond;

        public double StartTime => parentGameplayClock?.StartTime ?? 0;

        private readonly AudioAdjustments gameplayAdjustments = new AudioAdjustments();

        public IAdjustableAudioComponent AdjustmentsFromMods => parentGameplayClock?.AdjustmentsFromMods ?? gameplayAdjustments;

        #endregion

        #region Delegation of IFrameStableClock

        IBindable<bool> IFrameStableClock.IsCatchingUp => isCatchingUp;
        IBindable<bool> IFrameStableClock.WaitingOnFrames => waitingOnFrames;

        #endregion

        private enum PlaybackState
        {
            /// <summary>
            /// Playback is not possible. Child hierarchy should not be processed.
            /// </summary>
            NotValid,

            /// <summary>
            /// Playback is running behind real-time. Catch-up will be attempted by processing more than once per
            /// game loop (limited to a sane maximum to avoid frame drops).
            /// </summary>
            RequiresCatchUp,

            /// <summary>
            /// In a valid state, progressing one child hierarchy loop per game loop.
            /// </summary>
            Valid
        }
    }
}
