// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Beatmaps
{
    public class BeatmapModelDownloader : ModelDownloader<BeatmapSetInfo>
    {
        protected override ArchiveDownloadRequest<BeatmapSetInfo> CreateDownloadRequest(BeatmapSetInfo set, bool minimiseDownloadSize) =>
            new DownloadBeatmapSetRequest(set, minimiseDownloadSize);

        public BeatmapModelDownloader(IBeatmapModelManager beatmapModelManager, IAPIProvider api, GameHost host = null)
            : base(beatmapModelManager, api, host)
        {
        }
    }
}
