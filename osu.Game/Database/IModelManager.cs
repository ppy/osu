// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Database
{
    /// <summary>
    /// Represents a model manager that publishes events when <typeparamref name="TModel"/>s are added or removed.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelManager<TModel>
        where TModel : class
    {
        /// <summary>
        /// A bindable which contains a weak reference to the last item that was updated.
        /// This is not thread-safe and should be scheduled locally if consumed from a drawable component.
        /// </summary>
        IBindable<WeakReference<TModel>> ItemUpdated { get; }

        /// <summary>
        /// A bindable which contains a weak reference to the last item that was removed.
        /// This is not thread-safe and should be scheduled locally if consumed from a drawable component.
        /// </summary>
        IBindable<WeakReference<TModel>> ItemRemoved { get; }
    }
}
