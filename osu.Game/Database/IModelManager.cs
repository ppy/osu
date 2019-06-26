// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Database
{
    /// <summary>
    /// Represents a model manager that publishes events when <see cref="TModel"/>s are added or removed.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelManager<out TModel>
        where TModel : class
    {
        event Action<TModel> ItemAdded;

        event Action<TModel> ItemRemoved;
    }
}
