using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.IO;

namespace osu.Game.Database
{
    public class StableBeatmapImporter : StableImporter<BeatmapSetInfo>
    {
        protected override string ImportFromStablePath => ".";

        protected override Storage PrepareStableStorage(StableStorage stableStorage) => stableStorage.GetSongStorage();

        public StableBeatmapImporter(IModelImporter<BeatmapSetInfo> importer)
            : base(importer)
        {
        }
    }
}