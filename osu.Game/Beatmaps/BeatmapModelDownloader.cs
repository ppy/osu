// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Beatmaps
{
    public class BeatmapModelDownloader : ModelDownloader<BeatmapSetInfo, IBeatmapSetInfo>
    {
        protected override ArchiveDownloadRequest<IBeatmapSetInfo> CreateDownloadRequest(IBeatmapSetInfo set, bool minimiseDownloadSize) =>
            new DownloadBeatmapSetRequest(set, minimiseDownloadSize);

        public override ArchiveDownloadRequest<IBeatmapSetInfo>? GetExistingDownload(IBeatmapSetInfo model)
            => CurrentDownloads.Find(r => r.Model.OnlineID == model.OnlineID);

        public BeatmapModelDownloader(IModelImporter<BeatmapSetInfo> beatmapImporter, IAPIProvider api)
            : base(beatmapImporter, api)
        {
        }

        public bool Update(BeatmapSetInfo model)
        {
            return Download(model, false, onSuccess);

            void onSuccess(Live<BeatmapSetInfo> imported)
            {
                imported.PerformWrite(updated =>
                {
                    Logger.Log($"Beatmap \"{updated}\"update completed successfully", LoggingTarget.Database);

                    var original = updated.Realm.Find<BeatmapSetInfo>(model.ID);

                    // Generally the import process will do this for us if the OnlineIDs match,
                    // but that isn't a guarantee (ie. if the .osu file doesn't have OnlineIDs populated).
                    original.DeletePending = true;
                });
            }
        }
    }
}
