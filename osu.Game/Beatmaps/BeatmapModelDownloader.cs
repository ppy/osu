// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Beatmaps
{
    public class BeatmapModelDownloader : ModelDownloader<BeatmapSetInfo, IBeatmapSetInfo>
    {
        private readonly BeatmapManager beatmapManager;

        protected override ArchiveDownloadRequest<IBeatmapSetInfo> CreateDownloadRequest(IBeatmapSetInfo set, bool minimiseDownloadSize) =>
            new DownloadBeatmapSetRequest(set, minimiseDownloadSize);

        public override ArchiveDownloadRequest<IBeatmapSetInfo>? GetExistingDownload(IBeatmapSetInfo model)
            => CurrentDownloads.Find(r => r.Model.OnlineID == model.OnlineID);

        public BeatmapModelDownloader(BeatmapManager beatmapImporter, IAPIProvider api)
            : base(beatmapImporter, api)
        {
            beatmapManager = beatmapImporter;
        }

        protected override Task<IEnumerable<Live<BeatmapSetInfo>>> Import(ProgressNotification notification, string filename, BeatmapSetInfo? originalModel)
        {
            if (originalModel != null)
                return beatmapManager.ImportAsUpdate(notification, new ImportTask(filename), originalModel);

            return base.Import(notification, filename, null);
        }

        public bool Update(BeatmapSetInfo model) => Download(model, false, model);
    }
}
