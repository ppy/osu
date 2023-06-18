// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Listens to <see cref="IKeyBinding"/> events emitted by an <see cref="IKeybindingEventsEmitter"/>.
    /// Alternative to <see cref="IKeyBindingHandler{T}"/> for classes that need to not depend on type parameters.
    /// </summary>
    public interface IKeybindingListener
    {
        /// <summary>
        /// This class or a member of this class can already handle keybindings.
        /// Signals to the <see cref="IKeybindingEventsEmitter"/> that <see cref="OnPressed{T}"/> and <see cref="OnReleased{T}"/>
        /// don't necessarily need to be called.
        /// </summary>
        /// <remarks>
        /// This is usually true for <see cref="CompositeDrawable"/>s and <see cref="CompositeComponent"/>s that need to
        /// pass <see cref="IKeyBinding"/>s events to children that can already handle them.
        /// </remarks>
        public bool CanHandleKeybindings { get; }

        /// <summary>
        /// Prepares this class to receive events.
        /// </summary>
        /// <param name="actions">The list of possible actions that can occur.</param>
        /// <typeparam name="T">The type actions, commonly enums.</typeparam>
        public void Setup<T>(IEnumerable<T> actions) where T : struct;

        /// <summary>
        /// Called when an action is pressed.
        /// </summary>
        /// <param name="action">The event containing information about the pressed action.</param>
        /// <typeparam name="T">The type of binding, commonly enums.</typeparam>
        public void OnPressed<T>(KeyBindingPressEvent<T> action) where T : struct;

        /// <summary>
        /// Called when an action is released.
        /// </summary>
        /// <param name="action">The event containing information about the released action.</param>
        /// <typeparam name="T">The type of binding, commonly enums.</typeparam>
        public void OnReleased<T>(KeyBindingReleaseEvent<T> action) where T : struct;
    }
}
