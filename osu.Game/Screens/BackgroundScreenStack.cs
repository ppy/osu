// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Screens
{
    public partial class BackgroundScreenStack : ScreenStack
    {
        public BackgroundScreenStack()
            : base(false)
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        /// <summary>
        /// Attempt to push a new background screen to this stack.
        /// </summary>
        /// <param name="screen">The screen to attempt to push.</param>
        /// <returns>Whether the push succeeded. For example, if the existing screen was already of the correct type this will return <c>false</c>.</returns>
        public bool Push(BackgroundScreen? screen)
        {
            if (screen == null)
                return false;

            if (EqualityComparer<BackgroundScreen>.Default.Equals((BackgroundScreen)CurrentScreen, screen))
                return false;

            base.Push(screen);
            return true;
        }

        /// <summary>
        /// Schedules a delegate to run after 500ms, the time length of a background screen transition.
        /// This is used in <see cref="BackgroundScreenDefault"/> to dispose of the storyboard once the background screen is completely off-screen.
        /// </summary>
        /// <remarks>
        /// Late storyboard disposals cannot be achieved with any local scheduler from <see cref="BackgroundScreenDefault"/> or any component inside it,
        /// due to the screen becoming dead at the moment the transition finishes. And, on the frame that it is dead on, it will not receive an <see cref="Drawable.UpdateSubTree"/>,
        /// therefore not guaranteeing to dispose the storyboard at any period of time close to the end of the transition.
        /// This might require reconsideration framework-side, possibly exposing a "death" event in <see cref="Screen"/> or all <see cref="Drawable"/>s in general.
        /// </remarks>
        /// <param name="action">The delegate </param>
        /// <returns></returns>
        /// <seealso cref="BackgroundScreen.TRANSITION_LENGTH"/>
        internal ScheduledDelegate ScheduleUntilTransitionEnd(Action action) => Scheduler.AddDelayed(action, BackgroundScreen.TRANSITION_LENGTH);
    }
}
