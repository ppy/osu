// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
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
    public class FrameStabilityContainer : Container, IHasReplayHandler
    {
        private readonly double gameplayStartTime;

        /// <summary>
        /// The number of frames (per parent frame) which can be run in an attempt to catch-up to real-time.
        /// </summary>
        public int MaxCatchUpFrames { get; set; } = 5;

        /// <summary>
        /// Whether to enable frame-stable playback.
        /// </summary>
        internal bool FrameStablePlayback = true;

        public IFrameStableClock FrameStableClock => frameStableClock;

        [Cached(typeof(GameplayClock))]
        private readonly FrameStabilityClock frameStableClock;

        public FrameStabilityContainer(double gameplayStartTime = double.MinValue)
        {
            RelativeSizeAxes = Axes.Both;

            frameStableClock = new FrameStabilityClock(framedClock = new FramedClock(manualClock = new ManualClock()));

            this.gameplayStartTime = gameplayStartTime;
        }

        private readonly ManualClock manualClock;

        private readonly FramedClock framedClock;

        private IFrameBasedClock parentGameplayClock;

        /// <summary>
        /// The current direction of playback to be exposed to frame stable children.
        /// </summary>
        /// <remarks>
        /// Initially it is presumed that playback will proceed in the forward direction.
        /// </remarks>
        private int direction = 1;

        [BackgroundDependencyLoader(true)]
        private void load(GameplayClock clock)
        {
            if (clock != null)
            {
                parentGameplayClock = frameStableClock.ParentGameplayClock = clock;
                frameStableClock.IsPaused.BindTo(clock.IsPaused);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            setClock();
        }

        private PlaybackState state;

        protected override bool RequiresChildrenUpdate => base.RequiresChildrenUpdate && state != PlaybackState.NotValid;

        private bool hasReplayAttached => ReplayInputHandler != null;

        private const double sixty_frame_time = 1000.0 / 60;

        private bool firstConsumption = true;

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
            if (frameStableClock.WaitingOnFrames.Value)
            {
                // if waiting on frames, run one update loop to determine if frames have arrived.
                state = PlaybackState.Valid;
            }
            else if (frameStableClock.IsPaused.Value)
            {
                // time should not advance while paused, nor should anything run.
                state = PlaybackState.NotValid;
                return;
            }
            else
            {
                state = PlaybackState.Valid;
            }

            if (parentGameplayClock == null)
                setClock(); // LoadComplete may not be run yet, but we still want the clock.

            double proposedTime = parentGameplayClock.CurrentTime;

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

            double timeBehind = Math.Abs(proposedTime - parentGameplayClock.CurrentTime);

            frameStableClock.IsCatchingUp.Value = timeBehind > 200;
            frameStableClock.WaitingOnFrames.Value = state == PlaybackState.NotValid;

            manualClock.CurrentTime = proposedTime;
            manualClock.Rate = Math.Abs(parentGameplayClock.Rate) * direction;
            manualClock.IsRunning = parentGameplayClock.IsRunning;

            // determine whether catch-up is required.
            if (state == PlaybackState.Valid && timeBehind > 0)
                state = PlaybackState.RequiresCatchUp;

            // The manual clock time has changed in the above code. The framed clock now needs to be updated
            // to ensure that the its time is valid for our children before input is processed
            framedClock.ProcessFrame();
        }

        /// <summary>
        /// Attempt to advance replay playback for a given time.
        /// </summary>
        /// <param name="proposedTime">The time which is to be displayed.</param>
        /// <returns>Whether playback is still valid.</returns>
        private bool updateReplay(ref double proposedTime)
        {
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

        private void setClock()
        {
            if (parentGameplayClock == null)
            {
                // in case a parent gameplay clock isn't available, just use the parent clock.
                parentGameplayClock ??= Clock;
            }
            else
            {
                Clock = frameStableClock;
            }
        }

        public ReplayInputHandler ReplayInputHandler { get; set; }

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

        private class FrameStabilityClock : GameplayClock, IFrameStableClock
        {
            public GameplayClock ParentGameplayClock;

            public readonly Bindable<bool> IsCatchingUp = new Bindable<bool>();

            public readonly Bindable<bool> WaitingOnFrames = new Bindable<bool>();

            public override IEnumerable<Bindable<double>> NonGameplayAdjustments => ParentGameplayClock?.NonGameplayAdjustments ?? Enumerable.Empty<Bindable<double>>();

            public FrameStabilityClock(FramedClock underlyingClock)
                : base(underlyingClock)
            {
            }

            IBindable<bool> IFrameStableClock.IsCatchingUp => IsCatchingUp;

            IBindable<bool> IFrameStableClock.WaitingOnFrames => WaitingOnFrames;
        }
    }
}
