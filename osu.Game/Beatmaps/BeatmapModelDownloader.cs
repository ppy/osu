// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Beatmaps
{
    public class BeatmapModelDownloader : ModelDownloader<BeatmapSetInfo, IBeatmapSetInfo>
    {
        protected override ArchiveDownloadRequest<IBeatmapSetInfo> CreateDownloadRequest(IBeatmapSetInfo set, bool minimiseDownloadSize) =>
            new DownloadBeatmapSetRequest(set, minimiseDownloadSize);

        public override ArchiveDownloadRequest<IBeatmapSetInfo> GetExistingDownload(IBeatmapSetInfo model)
            => CurrentDownloads.Find(r => r.Model.OnlineID == model.OnlineID);

        public BeatmapModelDownloader(IModelImporter<BeatmapSetInfo> beatmapImporter, IAPIProvider api, GameHost host = null)
            : base(beatmapImporter, api, host)
        {
        }
    }
}
