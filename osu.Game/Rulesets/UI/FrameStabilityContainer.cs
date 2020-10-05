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

        public GameplayClock GameplayClock => stabilityGameplayClock;

        [Cached(typeof(GameplayClock))]
        [Cached(typeof(ISamplePlaybackDisabler))]
        private readonly StabilityGameplayClock stabilityGameplayClock;

        public FrameStabilityContainer(double gameplayStartTime = double.MinValue)
        {
            RelativeSizeAxes = Axes.Both;

            stabilityGameplayClock = new StabilityGameplayClock(framedClock = new FramedClock(manualClock = new ManualClock()));

            this.gameplayStartTime = gameplayStartTime;
        }

        private readonly ManualClock manualClock;

        private readonly FramedClock framedClock;

        private IFrameBasedClock parentGameplayClock;

        /// <summary>
        /// The current direction of playback to be exposed to frame stable children.
        /// </summary>
        private int direction;

        [BackgroundDependencyLoader(true)]
        private void load(GameplayClock clock, ISamplePlaybackDisabler sampleDisabler)
        {
            if (clock != null)
            {
                parentGameplayClock = stabilityGameplayClock.ParentGameplayClock = clock;
                GameplayClock.IsPaused.BindTo(clock.IsPaused);
            }

            // this is a bit temporary. should really be done inside of GameplayClock (but requires large structural changes).
            stabilityGameplayClock.ParentSampleDisabler = sampleDisabler;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            setClock();
        }

        /// <summary>
        /// Whether we are running up-to-date with our parent clock.
        /// If not, we will need to keep processing children until we catch up.
        /// </summary>
        private bool requireMoreUpdateLoops;

        /// <summary>
        /// Whether we are in a valid state (ie. should we keep processing children frames).
        /// This should be set to false when the replay is, for instance, waiting for future frames to arrive.
        /// </summary>
        private bool validState;

        protected override bool RequiresChildrenUpdate => base.RequiresChildrenUpdate && validState;

        private bool isAttached => ReplayInputHandler != null;

        private const double sixty_frame_time = 1000.0 / 60;

        private bool firstConsumption = true;

        public override bool UpdateSubTree()
        {
            requireMoreUpdateLoops = true;
            validState = !GameplayClock.IsPaused.Value;

            int loops = 0;

            while (validState && requireMoreUpdateLoops && loops++ < MaxCatchUpFrames)
            {
                updateClock();

                if (validState)
                {
                    base.UpdateSubTree();
                    UpdateSubTreeMasking(this, ScreenSpaceDrawQuad.AABBFloat);
                }
            }

            return true;
        }

        private void updateClock()
        {
            if (parentGameplayClock == null)
                setClock(); // LoadComplete may not be run yet, but we still want the clock.

            validState = true;
            requireMoreUpdateLoops = false;

            var newProposedTime = parentGameplayClock.CurrentTime;

            try
            {
                if (FrameStablePlayback)
                {
                    if (firstConsumption)
                    {
                        // On the first update, frame-stability seeking would result in unexpected/unwanted behaviour.
                        // Instead we perform an initial seek to the proposed time.

                        // process frame (in addition to finally clause) to clear out ElapsedTime
                        manualClock.CurrentTime = newProposedTime;
                        framedClock.ProcessFrame();

                        firstConsumption = false;
                    }
                    else if (manualClock.CurrentTime < gameplayStartTime)
                        manualClock.CurrentTime = newProposedTime = Math.Min(gameplayStartTime, newProposedTime);
                    else if (Math.Abs(manualClock.CurrentTime - newProposedTime) > sixty_frame_time * 1.2f)
                    {
                        newProposedTime = newProposedTime > manualClock.CurrentTime
                            ? Math.Min(newProposedTime, manualClock.CurrentTime + sixty_frame_time)
                            : Math.Max(newProposedTime, manualClock.CurrentTime - sixty_frame_time);
                    }
                }

                if (isAttached)
                {
                    double? newTime;

                    if (FrameStablePlayback)
                    {
                        // when stability is turned on, we shouldn't execute for time values the replay is unable to satisfy.
                        if ((newTime = ReplayInputHandler.SetFrameFromTime(newProposedTime)) == null)
                        {
                            // setting invalid state here ensures that gameplay will not continue (ie. our child
                            // hierarchy won't be updated).
                            validState = false;

                            // potentially loop to catch-up playback.
                            requireMoreUpdateLoops = true;

                            return;
                        }
                    }
                    else
                    {
                        // when stability is disabled, we don't really care about accuracy.
                        // looping over the replay will allow it to catch up and feed out the required values
                        // for the current time.
                        while ((newTime = ReplayInputHandler.SetFrameFromTime(newProposedTime)) != newProposedTime)
                        {
                            if (newTime == null)
                            {
                                // special case for when the replay actually can't arrive at the required time.
                                // protects from potential endless loop.
                                validState = false;
                                return;
                            }
                        }
                    }

                    newProposedTime = newTime.Value;
                }
            }
            finally
            {
                if (newProposedTime != manualClock.CurrentTime)
                    direction = newProposedTime > manualClock.CurrentTime ? 1 : -1;

                manualClock.CurrentTime = newProposedTime;
                manualClock.Rate = Math.Abs(parentGameplayClock.Rate) * direction;
                manualClock.IsRunning = parentGameplayClock.IsRunning;

                requireMoreUpdateLoops |= manualClock.CurrentTime != parentGameplayClock.CurrentTime;

                // The manual clock time has changed in the above code. The framed clock now needs to be updated
                // to ensure that the its time is valid for our children before input is processed
                framedClock.ProcessFrame();
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
                Clock = GameplayClock;
            }
        }

        public ReplayInputHandler ReplayInputHandler { get; set; }

        private class StabilityGameplayClock : GameplayClock
        {
            public GameplayClock ParentGameplayClock;

            public ISamplePlaybackDisabler ParentSampleDisabler;

            public override IEnumerable<Bindable<double>> NonGameplayAdjustments => ParentGameplayClock?.NonGameplayAdjustments ?? Enumerable.Empty<Bindable<double>>();

            public StabilityGameplayClock(FramedClock underlyingClock)
                : base(underlyingClock)
            {
            }

            protected override bool ShouldDisableSamplePlayback =>
                // handle the case where playback is catching up to real-time.
                base.ShouldDisableSamplePlayback
                || ParentSampleDisabler?.SamplePlaybackDisabled.Value == true
                || (ParentGameplayClock != null && Math.Abs(CurrentTime - ParentGameplayClock.CurrentTime) > 200);
        }
    }
}
