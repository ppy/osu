// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Tournament.Screens.Editors
{
    /// <summary>
    /// Provides a mechanism to access a related model from a representing class.
    /// </summary>
    /// <typeparam name="TModel">The type of model.</typeparam>
    public interface IModelBacked<out TModel>
    {
        /// <summary>
        /// The model.
        /// </summary>
        TModel Model { get; }
    }
}
