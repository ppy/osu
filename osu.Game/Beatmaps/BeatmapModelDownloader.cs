// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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

                    foreach (var beatmap in original.Beatmaps.ToArray())
                    {
                        var updatedBeatmap = updated.Beatmaps.FirstOrDefault(b => b.Hash == beatmap.Hash);

                        if (updatedBeatmap != null)
                        {
                            // If the updated beatmap matches an existing one, transfer any user data across..
                            if (beatmap.Scores.Any())
                            {
                                Logger.Log($"Transferring {beatmap.Scores.Count()} scores for unchanged difficulty \"{beatmap}\"", LoggingTarget.Database);

                                foreach (var score in beatmap.Scores)
                                    score.BeatmapInfo = updatedBeatmap;
                            }

                            // ..then nuke the old beatmap completely.
                            // this is done instead of a soft deletion to avoid a user potentially creating weird
                            // interactions, like restoring the outdated beatmap then updating a second time
                            // (causing user data to be wiped).
                            original.Beatmaps.Remove(beatmap);
                        }
                        else
                        {
                            // If the beatmap differs in the original, leave it in a soft-deleted state but reset online info.
                            // This caters to the case where a user has made modifications they potentially want to restore,
                            // but after restoring we want to ensure it can't be used to trigger an update of the beatmap.
                            beatmap.ResetOnlineInfo();
                        }
                    }
                });
            }
        }
    }
}
