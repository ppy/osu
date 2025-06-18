// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Skinning;

namespace osu.Game.Screens.Menu
{
    public partial class StarFountainSounds : CompositeComponent
    {
        private const int shoot_retrigger_delay = 500;
        private const int loop_fade_duration = 500;

        private double? lastPlayback;

        private SkinnableSound shootSample = null!;
        private PausableSkinnableSound loopSample = null!;

        private ScheduledDelegate? loopFadeDelegate;
        private ScheduledDelegate? loopStopDelegate;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                shootSample = new SkinnableSound(new SampleInfo("Gameplay/fountain-shoot")),
                loopSample = new PausableSkinnableSound(new SampleInfo("Gameplay/fountain-loop")) { Looping = true },
            };
        }

        public void Play()
        {
            loopFadeDelegate?.Cancel();
            loopStopDelegate?.Cancel();

            try
            {
                // Only play 'shootSample' if enough time has passed since last `Play()` call.
                if (lastPlayback == null || Time.Current - lastPlayback > shoot_retrigger_delay)
                {
                    loopSample.Stop();
                    shootSample.Play();
                    return;
                }

                // Only call `Play()` if `loopSample` is not already playing, to prevent restarting the sample each time.
                if (!loopSample.RequestedPlaying)
                {
                    this.TransformBindableTo(loopSample.Volume, 1);
                    loopSample.Play();
                }

                // Schedule a volume fadeout, followed by a `Stop()`.
                loopFadeDelegate = Scheduler.AddDelayed(() =>
                {
                    this.TransformBindableTo(loopSample.Volume, 0, loop_fade_duration);
                    loopStopDelegate = Scheduler.AddDelayed(() => loopSample.Stop(), loop_fade_duration);
                }, shoot_retrigger_delay);
            }
            finally
            {
                lastPlayback = Time.Current;
            }
        }
    }
}
