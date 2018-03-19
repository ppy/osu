using OpenTK;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Vitaru.Objects.Drawables;

namespace osu.Game.Rulesets.Vitaru.Judgements
{
    public class VitaruJudgement : Judgement
    {
        /// <summary>
        /// The positional hit offset.
        /// </summary>
        public Vector2 PositionOffset;

        public ComboResult Combo;
    }
}
