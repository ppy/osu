// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Screens.Select;

namespace osu.Game.Beatmaps
{
    public class BeatmapModelDownloader : ModelDownloader<BeatmapSetInfo, IBeatmapSetInfo>
    {
        private readonly BeatmapManager? beatmapManager;

        protected override ArchiveDownloadRequest<IBeatmapSetInfo> CreateDownloadRequest(IBeatmapSetInfo set, bool minimiseDownloadSize) =>
            new DownloadBeatmapSetRequest(set, minimiseDownloadSize);

        public override ArchiveDownloadRequest<IBeatmapSetInfo>? GetExistingDownload(IBeatmapSetInfo model)
            => CurrentDownloads.Find(r => r.Model.OnlineID == model.OnlineID);

        public BeatmapModelDownloader(BeatmapManager beatmapImporter, IAPIProvider api)
            : base(beatmapImporter, api)
        {
            beatmapManager = beatmapImporter;
        }

        public void Download(IBeatmapSetInfo model, bool withoutVideo, IDialogOverlay? dialogOverlay = null)
        {
            if (dialogOverlay != null)
            {
                BeatmapSetInfo? localModel = model as BeatmapSetInfo ?? beatmapManager?.QueryBeatmapSet(s => s.OnlineID == model.OnlineID)?.Value;

                if (localModel?.Status == BeatmapOnlineStatus.LocallyModified)
                {
                    dialogOverlay.Push(new UpdateLocalConfirmationDialog(() =>
                    {
                        DownloadAsUpdate(localModel, withoutVideo);
                    }));

                    return;
                }
            }

            base.Download(model, withoutVideo, null);
        }

        public void DownloadAsUpdate(BeatmapSetInfo originalModel, bool withoutVideo) => Download(originalModel, withoutVideo, originalModel);
    }
}
