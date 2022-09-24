// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Beatmaps
{
    public class BeatmapModelDownloader : ModelDownloader<BeatmapSetInfo, IBeatmapSetInfo>
    {
        protected override ArchiveDownloadRequest<IBeatmapSetInfo> CreateDownloadRequest(IBeatmapSetInfo set, bool minimiseDownloadSize) =>
            new DownloadBeatmapSetRequest(set, minimiseDownloadSize);

        protected override ArchiveDownloadRequest<IBeatmapSetInfo>? CreateAccelDownloadRequest(IBeatmapSetInfo model, bool isMini)
        {
            try
            {
                return new AccelDownloadBeatmapSetRequest(model, isMini);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"未能创建加速下载请求, 将尝试从官网下载: {e.Message}");
                return null;
            }
        }

        public override ArchiveDownloadRequest<IBeatmapSetInfo>? GetExistingDownload(IBeatmapSetInfo model)
            => CurrentDownloads.Find(r => r.Model.OnlineID == model.OnlineID);

        public BeatmapModelDownloader(IModelImporter<BeatmapSetInfo> beatmapImporter, IAPIProvider api)
            : base(beatmapImporter, api)
        {
        }
    }
}
