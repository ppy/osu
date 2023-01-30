// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;
using DefaultJudgementPiece = osu.Game.Rulesets.Taiko.Skinning.Default.DefaultJudgementPiece;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// Text that is shown as judgement when a hit object is hit or missed.
    /// </summary>
    public partial class DrawableTaikoJudgement : DrawableJudgement
    {
        public DrawableTaikoJudgement()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = Vector2.One;
        }

        protected override Drawable CreateDefaultJudgement(HitResult result) => new DefaultJudgementPiece(result);
    }
}
