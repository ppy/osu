using osu.Game.Scoring;
namespace osu.Game.Online.API.Requests
{
    public class DownloadReplayRequest : ArchiveDownloadModelRequest<ScoreInfo>
    {
        public DownloadReplayRequest(ScoreInfo score)
            : base(score)
        {
        }

        protected override string Target => $@"scores/{Info.Ruleset.ShortName}/{Info.OnlineScoreID}/download";
    }
}
