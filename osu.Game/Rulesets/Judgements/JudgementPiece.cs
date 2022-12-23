// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
    public abstract partial class JudgementPiece : CompositeDrawable
    {
        protected readonly HitResult Result;

        protected SpriteText JudgementText { get; set; } = null!;

        protected JudgementPiece(HitResult result)
        {
            Result = result;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            JudgementText.Text = Result.GetDescription().ToUpperInvariant();
        }
    }
}
