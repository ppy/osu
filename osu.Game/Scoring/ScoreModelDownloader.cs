// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Scoring
{
    public class ScoreModelDownloader : ModelDownloader<ScoreInfo>
    {
        public ScoreModelDownloader(IModelImporter<ScoreInfo> scoreManager, IAPIProvider api, IIpcHost importHost = null)
            : base(scoreManager, api, importHost)
        {
        }

        protected override ArchiveDownloadRequest<ScoreInfo> CreateDownloadRequest(ScoreInfo score, bool minimiseDownload) => new DownloadReplayRequest(score);

        public override ArchiveDownloadRequest<ScoreInfo> GetExistingDownload(ScoreInfo model)
            => CurrentDownloads.Find(r => r.Model.OnlineScoreID == model.OnlineScoreID);
    }
}
