namespace osu.Game.Modes.Taiko.Judgements
{
    public class TaikoDrumRollTickJudgementInfo : TaikoJudgementInfo
    {
        protected override int ScoreToInt(TaikoScoreResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case TaikoScoreResult.Great:
                    return 200;
            }
        }

        protected override int AccuracyScoreToInt(TaikoScoreResult result)
        {
            return 0;
        }
    }
}
