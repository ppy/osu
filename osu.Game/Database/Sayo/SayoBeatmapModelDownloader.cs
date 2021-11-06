// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Online.API;

namespace osu.Game.Database.Sayo
{
    public class SayoBeatmapModelDownloader : SayoModelDownloader<BeatmapSetInfo>
    {
        public SayoBeatmapModelDownloader(IBeatmapModelManager beatmapModelManager, IAPIProvider api, GameHost host = null)
            : base(beatmapModelManager, api, host)
        {
        }

        protected override ArchiveDownloadRequest<BeatmapSetInfo> CreateSayoDownloadRequest(BeatmapSetInfo model, bool noVideo, bool isMini)
            => new SayoDownloadBeatmapSetRequest(model, noVideo, isMini);
    }
}
