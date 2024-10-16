// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Represents a single property update on a given <see cref="Target"/>.
    /// </summary>
    /// <typeparam name="TTarget">Type of the object owning the property</typeparam>
    /// <typeparam name="TValue">Type of the property to update</typeparam>
    public abstract class PropertyChange<TTarget, TValue> : IRevertableChange where TTarget : class
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
        /// The target object, which owns the property to change.
        /// </summary>
        public readonly TTarget Target;

        /// <summary>
        /// The value to change the property to.
        /// </summary>
        public readonly TValue Value;

        /// <summary>
        /// The original value of the property before the change.
        /// </summary>
        public readonly TValue OriginalValue;

        protected PropertyChange(TTarget target, TValue value)
        {
            Target = target;
            Value = value;
            OriginalValue = ReadValue(target);
        }

        public void Apply() => WriteValue(Target, Value);

        public void Revert() => WriteValue(Target, OriginalValue);
    }
}
