// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Select;

namespace osu.Game.Beatmaps
{
    public class BeatmapModelDownloader : ModelDownloader<BeatmapSetInfo, IBeatmapSetInfo>
    {
        private readonly BeatmapManager? beatmapManager;

        protected override ArchiveDownloadRequest<IBeatmapSetInfo> CreateDownloadRequest(IBeatmapSetInfo set) =>
            new DownloadBeatmapSetRequest(set, MinimiseDownloadSize.Value);

        public override ArchiveDownloadRequest<IBeatmapSetInfo>? GetExistingDownload(IBeatmapSetInfo model)
            => CurrentDownloads.Find(r => r.Model.OnlineID == model.OnlineID);

        public BeatmapModelDownloader(BeatmapManager beatmapImporter, IAPIProvider api)
            : base(beatmapImporter, api)
        {
            beatmapManager = beatmapImporter;
        }

        public void Download(IBeatmapSetInfo model, bool isAutomaticDownload = false)
        {
            if (PresentDialog != null)
            {
                BeatmapSetInfo? localModel = model as BeatmapSetInfo ?? beatmapManager?.QueryBeatmapSet(s => s.OnlineID == model.OnlineID)?.Value.Detach();

                if (localModel?.Status == BeatmapOnlineStatus.LocallyModified)
                {
                    if (isAutomaticDownload)
                    {
                        PostNotification?.Invoke(new SimpleNotification
                        {
                            Text = "Automatic download was aborted because local changes exist for the requested beatmap. Please check and start a manual download.",
                            Icon = FontAwesome.Solid.ExclamationTriangle,
                        });
                    }
                    else
                    {
                        PresentDialog(new UpdateLocalConfirmationDialog(() =>
                        {
                            DownloadAsUpdate(localModel);
                        }));
                    }

                    return;
                }
            }

            base.Download(model, null);
        }

        public void DownloadAsUpdate(BeatmapSetInfo originalModel) => Download(originalModel, originalModel);
    }
}
