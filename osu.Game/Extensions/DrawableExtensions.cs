// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Threading;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Extensions
{
    public static class DrawableExtensions
    {
        public const double REPEAT_INTERVAL = 70;
        public const double INITIAL_DELAY = 250;

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
        /// <param name="initialRepeatDelay">The delay imposed on the first repeat. Defaults to <see cref="INITIAL_DELAY"/>.</param>
        /// <returns>A <see cref="ScheduledDelegate"/> which can be cancelled to stop the repeat events from firing.</returns>
        public static ScheduledDelegate BeginKeyRepeat(this IKeyBindingHandler handler, Scheduler scheduler, Action action, double initialRepeatDelay = INITIAL_DELAY)
        {
            action();

            ScheduledDelegate repeatDelegate = new ScheduledDelegate(action, handler.Time.Current + initialRepeatDelay, REPEAT_INTERVAL);
            scheduler.Add(repeatDelegate);
            return repeatDelegate;
        }

        /// <summary>
        /// Accepts a delta vector in screen-space coordinates and converts it to one which can be applied to this drawable's position.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        /// <param name="delta">A delta in screen-space coordinates.</param>
        /// <returns>The delta vector in Parent's coordinates.</returns>
        public static Vector2 ScreenSpaceDeltaToParentSpace(this Drawable drawable, Vector2 delta) =>
            drawable.Parent.ToLocalSpace(drawable.Parent.ToScreenSpace(Vector2.Zero) + delta);

        public static SkinnableInfo CreateSkinnableInfo(this Drawable component) => new SkinnableInfo(component);

        public static void ApplySkinnableInfo(this Drawable component, SkinnableInfo info)
        {
            // todo: can probably make this better via deserialisation directly using a common interface.
            component.Position = info.Position;
            component.Rotation = info.Rotation;
            component.Scale = info.Scale;
            component.Anchor = info.Anchor;
            component.Origin = info.Origin;

            if (component is ISkinnableDrawable skinnable)
                skinnable.UsesFixedAnchor = info.UsesFixedAnchor;

            if (component is Container container)
            {
                foreach (var child in info.Children)
                    container.Add(child.CreateInstance());
            }
        }
    }
}
