using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.UI
{
    public class SpeedAdjustedPlayfield<TObject, TJudgement> : Playfield<TObject, TJudgement>
        where TObject : HitObject
        where TJudgement : Judgement
    {
        protected SpeedAdjustedPlayfield(float? customWidth = null)
            : base(customWidth)
        {
        }
    }
}