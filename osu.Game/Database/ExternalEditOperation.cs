// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading.Tasks;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public class ExternalEditOperation<TModel> where TModel : class, IHasGuidPrimaryKey
    {
        public readonly string MountedPath;

        private readonly IModelImporter<TModel> importer;
        private readonly TModel original;

        private bool isMounted;

        public ExternalEditOperation(IModelImporter<TModel> importer, TModel original, string path)
        {
            this.importer = importer;
            this.original = original;

            MountedPath = path;

            isMounted = true;
        }

        public async Task<Live<TModel>?> Finish()
        {
            if (!Directory.Exists(MountedPath) || !isMounted)
                return null;

            Live<TModel>? imported = await importer.ImportAsUpdate(new ProgressNotification(), new ImportTask(MountedPath), original)
                                                   .ConfigureAwait(false);

            try
            {
                Directory.Delete(MountedPath, true);
            }
            catch { }

            isMounted = false;

            return imported;
        }
    }
}
