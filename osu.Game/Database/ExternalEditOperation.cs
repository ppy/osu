// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading.Tasks;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    /// <summary>
    /// Contains information related to an active external edit operation.
    /// </summary>
    public class ExternalEditOperation<TModel> where TModel : class, IHasGuidPrimaryKey
    {
        /// <summary>
        /// The temporary path at which the model has been exported to for editing.
        /// </summary>
        public readonly string MountedPath;

        /// <summary>
        /// Whether the model is still mounted at <see cref="MountedPath"/>.
        /// </summary>
        public bool IsMounted { get; private set; }

        private readonly IModelImporter<TModel> importer;
        private readonly TModel original;

        public ExternalEditOperation(IModelImporter<TModel> importer, TModel original, string path)
        {
            this.importer = importer;
            this.original = original;

            MountedPath = path;

            IsMounted = true;
        }

        /// <summary>
        /// Finish the external edit operation.
        /// </summary>
        /// <remarks>
        /// This will trigger an asynchronous reimport of the model.
        /// Subsequent calls will be a no-op.
        /// </remarks>
        /// <returns>A task which will eventuate in the newly imported model with changes applied.</returns>
        public async Task<Live<TModel>?> Finish()
        {
            if (!Directory.Exists(MountedPath) || !IsMounted)
                return null;

            IsMounted = false;

            Live<TModel>? imported = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(MountedPath), original)
                                                   .ConfigureAwait(false);

            try
            {
                Directory.Delete(MountedPath, true);
            }
            catch { }

            return imported;
        }
    }
}
