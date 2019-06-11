
using System;

namespace osu.Game.Database
{
    public interface IModelManager<TModel>
    {
        /// <summary>
        /// Fired when a new <see cref="TModel"/> becomes available in the database.
        /// This is not guaranteed to run on the update thread.
        /// </summary>
        event Action<TModel, bool> ItemAdded;

        /// <summary>
        /// Fired when a <see cref="TModel"/> is removed from the database.
        /// This is not guaranteed to run on the update thread.
        /// </summary>
        event Action<TModel> ItemRemoved;
    }
}
