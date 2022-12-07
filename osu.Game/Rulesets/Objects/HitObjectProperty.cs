// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// Represents a wrapper containing a lazily-initialised <see cref="Bindable{T}"/>, backed by a temporary field used for <see cref="Value"/> storage until initialisation.
    /// </summary>
    public struct HitObjectProperty<T>
    {
        [CanBeNull]
        private Bindable<T> backingBindable;

        /// <summary>
        /// A temporary field to store the current value to, prior to <see cref="Bindable"/>'s initialisation.
        /// </summary>
        private T backingValue;

        /// <summary>
        /// The underlying <see cref="Bindable{T}"/>, only initialised on first access.
        /// </summary>
        public Bindable<T> Bindable => backingBindable ??= new Bindable<T>(defaultValue) { Value = backingValue };

        /// <summary>
        /// The current value, derived from and delegated to <see cref="Bindable"/> if initialised, or a temporary field otherwise.
        /// </summary>
        public T Value
        {
            get => backingBindable != null ? backingBindable.Value : backingValue;
            set
            {
                if (backingBindable != null)
                    backingBindable.Value = value;
                else
                    backingValue = value;
            }
        }

        private readonly T defaultValue;

        public HitObjectProperty(T value = default)
        {
            backingValue = defaultValue = value;
            backingBindable = null;
        }
    }
}
