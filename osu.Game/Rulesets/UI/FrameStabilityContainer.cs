// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Input.Handlers;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A container which consumes a parent gameplay clock and standardises frame counts for children.
    /// Will ensure a minimum of 40 frames per clock second is maintained, regardless of any system lag or seeks.
    /// </summary>
    public class FrameStabilityContainer : Container, IHasReplayHandler
    {
        public FrameStabilityContainer()
        {
            RelativeSizeAxes = Axes.Both;
            gameplayClock = new GameplayClock(framedClock = new FramedClock(manualClock = new ManualClock()));
        }

        private readonly ManualClock manualClock;

        private readonly FramedClock framedClock;

        [Cached]
        private GameplayClock gameplayClock;

        private IFrameBasedClock parentGameplayClock;

        [BackgroundDependencyLoader(true)]
        private void load(GameplayClock clock)
        {
            if (clock != null)
            {
                parentGameplayClock = clock;
                gameplayClock.IsPaused.BindTo(clock.IsPaused);
            }
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

        private const int max_catch_up_updates_per_frame = 50;

        private const double sixty_frame_time = 1000.0 / 60;

        public override bool UpdateSubTree()
        {
            requireMoreUpdateLoops = true;
            validState = !gameplayClock.IsPaused.Value;

            int loops = 0;

            while (validState && requireMoreUpdateLoops && loops++ < max_catch_up_updates_per_frame)
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

            manualClock.Rate = parentGameplayClock.Rate;
            manualClock.IsRunning = parentGameplayClock.IsRunning;

            var newProposedTime = parentGameplayClock.CurrentTime;

            try
            {
                if (Math.Abs(manualClock.CurrentTime - newProposedTime) > sixty_frame_time * 1.2f)
                {
                    newProposedTime = manualClock.Rate > 0
                        ? Math.Min(newProposedTime, manualClock.CurrentTime + sixty_frame_time)
                        : Math.Max(newProposedTime, manualClock.CurrentTime - sixty_frame_time);
                }

                if (!isAttached)
                {
                    manualClock.CurrentTime = newProposedTime;
                }
                else
                {
                    double? newTime = ReplayInputHandler.SetFrameFromTime(newProposedTime);

                    if (newTime == null)
                    {
                        // we shouldn't execute for this time value. probably waiting on more replay data.
                        validState = false;

                        requireMoreUpdateLoops = true;
                        manualClock.CurrentTime = newProposedTime;
                        return;
                    }

                    manualClock.CurrentTime = newTime.Value;
                }

                requireMoreUpdateLoops = manualClock.CurrentTime != parentGameplayClock.CurrentTime;
            }
            finally
            {
                // The manual clock time has changed in the above code. The framed clock now needs to be updated
                // to ensure that the its time is valid for our children before input is processed
                framedClock.ProcessFrame();
            }
        }

        private void setClock()
        {
            // in case a parent gameplay clock isn't available, just use the parent clock.
            if (parentGameplayClock == null)
                parentGameplayClock = Clock;

            Clock = gameplayClock;
            ProcessCustomClock = false;
        }

        public ReplayInputHandler ReplayInputHandler { get; set; }
    }
}
