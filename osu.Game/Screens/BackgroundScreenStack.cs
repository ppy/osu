// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Storyboards.Drawables;

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

        public void ScheduleStoryboardDisposal(DrawableStoryboard storyboard) => Scheduler.AddDelayed(storyboard.RemoveAndDisposeImmediately, BackgroundScreen.TRANSITION_LENGTH);
    }
}
