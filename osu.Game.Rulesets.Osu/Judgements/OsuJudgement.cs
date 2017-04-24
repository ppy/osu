// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Framework.Extensions;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuJudgement : Judgement
    {
        /// <summary>
        /// The positional hit offset.
        /// </summary>
        public Vector2 PositionOffset;

        /// <summary>
        /// The score the user achieved.
        /// </summary>
        public OsuScoreResult Score;

        /// <summary>
        /// The score which would be achievable on a perfect hit.
        /// </summary>
        public OsuScoreResult MaxScore = OsuScoreResult.Hit300;

        public override string ResultString => Score.GetDescription();

        public override string MaxResultString => MaxScore.GetDescription();

        public int ScoreValue => scoreToInt(Score);

        public int MaxScoreValue => scoreToInt(MaxScore);

        private int scoreToInt(OsuScoreResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case OsuScoreResult.Hit50:
                    return 50;
                case OsuScoreResult.Hit100:
                    return 100;
                case OsuScoreResult.Hit300:
                    return 300;
                case OsuScoreResult.SliderTick:
                    return 10;
            }
        }

        public ComboResult Combo;
    }
}