// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using osu.Framework.Bindables;
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
            component.UsingClosestAnchor().Value = info.UsingClosestAnchor;

            if (component is Container container)
            {
                foreach (var child in info.Children)
                    container.Add(child.CreateInstance());
            }
        }

        /// <remarks>
        /// <p>A <see cref="ConditionalWeakTable{TKey,TValue}">ConditionalWeakTable</see> is preferable to a <see cref="Dictionary{TKey,TValue}">Dictionary</see> because a <c>Dictionary</c> will keep
        /// orphaned references to an <see cref="ISkinnableDrawable"/> forever, unless manually pruned.</p>
        /// <p><see cref="BindableBool"/> is used as a thin wrapper around <see cref="System.Boolean">bool</see> because <c>ConditionalWeakTable</c> requires a reference type as both a key and a value.</p>
        /// <p><see cref="IDrawable"/> was chosen over <see cref="Drawable"/> because it is a common ancestor between <see cref="Drawable"/> (which is required for <see cref="Drawable.Anchor"/> logic)
        /// and <see cref="ISkinnableDrawable"/> (which is required for serialization via <see cref="SkinnableInfo"/>).</p>
        /// <p>This collection is thread-safe according to the
        /// <a href="https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.conditionalweaktable-2?view=net-5.0#thread-safety">documentation</a>,
        /// but the <c>BindableBool</c>s are not unless <see cref="Bindable{T}.BeginLease">leased</see>.</p>
        /// </remarks>
        private static readonly ConditionalWeakTable<IDrawable, BindableBool> is_drawable_using_closest_anchor_lookup = new ConditionalWeakTable<IDrawable, BindableBool>();

        /// <summary>
        /// Gets or creates a <see cref="BindableBool"/> representing whether <paramref name="drawable"/> is using the closest <see cref="Drawable.Anchor">anchor point</see> within its
        /// <see cref="Drawable.Parent">parent</see>.
        /// </summary>
        /// <returns>A <see cref="BindableBool"/> whose <see cref="Bindable{T}.Value"/> is <see langword="true"/> if the <see cref="IDrawable"/> is using the closest anchor point,
        /// otherwise <see langword="false"/>.</returns>
        public static BindableBool UsingClosestAnchor(this IDrawable drawable) =>
            is_drawable_using_closest_anchor_lookup.GetValue(drawable, _ => new BindableBool(true));
    }
}
