// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Scoring
{
    public class ScoreModelDownloader : ModelDownloader<ScoreInfo, IScoreInfo>
    {
        public ScoreModelDownloader(IModelImporter<ScoreInfo> scoreManager, IAPIProvider api)
            : base(scoreManager, api)
        {
        }

        protected override ArchiveDownloadRequest<IScoreInfo> CreateDownloadRequest(IScoreInfo score, bool minimiseDownload) => new DownloadReplayRequest(score);

        public override ArchiveDownloadRequest<IScoreInfo> GetExistingDownload(IScoreInfo model)
            => CurrentDownloads.Find(r => r.Model.MatchesOnlineID(model));
    }
}
