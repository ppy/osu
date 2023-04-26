// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    /// <summary>
    /// A class which handles importing of associated models to the game store.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelImporter<TModel> : IPostNotifications, ICanAcceptFiles
        where TModel : class, IHasGuidPrimaryKey
    {
        /// <summary>
        /// Process multiple import tasks, updating a tracking notification with progress.
        /// </summary>
        /// <param name="notification">The notification to update.</param>
        /// <param name="tasks">The import tasks.</param>
        /// <param name="parameters">Parameters to further configure the import process.</param>
        /// <returns>The imported models.</returns>
        Task<IEnumerable<Live<TModel>>> Import(ProgressNotification notification, ImportTask[] tasks, ImportParameters parameters = default);

        /// <summary>
        /// Process a single import as an update for an existing model.
        /// This will still run a full import, but perform any post-processing required to make it feel like an update to the user.
        /// </summary>
        /// <param name="notification">The notification to update.</param>
        /// <param name="task">The import task.</param>
        /// <param name="original">The original model which is being updated.</param>
        /// <returns>The imported model.</returns>
        Task<Live<TModel>?> ImportAsUpdate(ProgressNotification notification, ImportTask task, TModel original);

        /// <summary>
        /// A user displayable name for the model type associated with this manager.
        /// </summary>
        string HumanisedModelName => $"{typeof(TModel).Name.Replace(@"Info", "").ToLowerInvariant()}";

        /// <summary>
        /// Fired when the user requests to view the resulting import.
        /// </summary>
        public Action<IEnumerable<Live<TModel>>>? PresentImport { set; }
    }
}
