using System;
using OpenTK;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Shape.Objects.Drawables;
using osu.Framework.Extensions;

namespace osu.Game.Rulesets.Shape.Judgements
{
    public class ShapeJudgement : Judgement
    {
        /// <summary>
        /// The positional hit offset.
        /// </summary>
        public Vector2 PositionOffset;

        public ComboResult Combo;
    }
}