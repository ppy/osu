// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Database
{
    public interface IModelManager<TModel>
    {
        event Action<TModel, bool> ItemAdded;

        event Action<TModel> ItemRemoved;
    }
}
