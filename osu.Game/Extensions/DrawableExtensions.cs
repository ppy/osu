// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Bindings;
using osu.Framework.Threading;

namespace osu.Game.Extensions
{
    public static class DrawableExtensions
    {
        /// <summary>
        /// Helper method that is used while <see cref="IKeyBindingHandler"/> doesn't support repetitions of <see cref="IKeyBindingHandler{T}.OnPressed"/>.
        /// Simulates repetitions by continually invoking a delegate according to the default key repeat rate.
        /// </summary>
        /// <remarks>
        /// The returned delegate can be cancelled to stop repeat events from firing (usually in <see cref="IKeyBindingHandler{T}.OnReleased"/>).
        /// </remarks>
        /// <param name="handler">The <see cref="IKeyBindingHandler{T}"/> which is handling the repeat.</param>
        /// <param name="scheduler">The <see cref="Scheduler"/> to schedule repetitions on.</param>
        /// <param name="action">The <see cref="Action"/> to be invoked once immediately and with every repetition.</param>
        /// <returns>A <see cref="ScheduledDelegate"/> which can be cancelled to stop the repeat events from firing.</returns>
        public static ScheduledDelegate BeginKeyRepeat(this IKeyBindingHandler handler, Scheduler scheduler, Action action)
        {
            action();

            ScheduledDelegate repeatDelegate = new ScheduledDelegate(action, handler.Time.Current + 250, 70);
            scheduler.Add(repeatDelegate);
            return repeatDelegate;
        }
    }
}
