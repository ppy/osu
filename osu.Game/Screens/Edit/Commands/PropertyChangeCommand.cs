// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace osu.Game.Screens.Edit.Commands
{
    /// <summary>
    /// Represents a single property update on a given <see cref="TTarget"/>.
    /// </summary>
    /// <typeparam name="TTarget">Type of the object owning the property</typeparam>
    /// <typeparam name="TValue">Type of the property to update</typeparam>
    public abstract class PropertyChangeCommand<TTarget, TValue> : IMergeableCommand where TTarget : class
    {
        /// <summary>
        /// Reads the current value of the property from the target.
        /// </summary>
        protected abstract TValue ReadValue(TTarget target);

        /// <summary>
        /// Writes the new value to the target object.
        /// </summary>
        protected abstract void WriteValue(TTarget target, TValue value);

        /// <summary>
        /// Creates a new instance of this <see cref="PropertyChangeCommand{TTarget,TProperty}"/> for use in <see cref="CreateUndo"/>
        /// The return instance must match the most derived type of the command class this method is implemented on.
        /// </summary>
        /// <param name="target">Target of the command</param>
        /// <param name="value">Value of the command</param>
        /// <returns></returns>
        protected abstract PropertyChangeCommand<TTarget, TValue> CreateInstance(TTarget target, TValue value);

        /// <summary>
        /// The target object, which owns the property to change.
        /// </summary>
        public readonly TTarget Target;

        /// <summary>
        /// The value to change the property to.
        /// </summary>
        public readonly TValue Value;

        protected PropertyChangeCommand(TTarget target, TValue value)
        {
            Target = target;
            Value = value;
        }

        public void Apply() => WriteValue(Target, Value);

        public IEditorCommand CreateUndo() => CreateInstance(Target, ReadValue(Target));

        public bool IsRedundant => ValueEquals(Value, ReadValue(Target));

        protected virtual bool ValueEquals(TValue a, TValue b) => EqualityComparer<TValue>.Default.Equals(a, b);

        private bool canMergeWith(IEditorCommand command) => command.GetType() == GetType() && ((PropertyChangeCommand<TTarget, TValue>)command).Target == Target;

        public bool MergeWithPrevious(IEditorCommand previousCommand, [MaybeNullWhen(false)] out IEditorCommand merged)
        {
            if (canMergeWith(previousCommand))
            {
                merged = this;
                return true;
            }

            merged = null;
            return false;
        }
    }
}
