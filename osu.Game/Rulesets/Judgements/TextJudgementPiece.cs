// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
    public abstract partial class TextJudgementPiece : CompositeDrawable
    {
        protected readonly HitResult Result;

        protected SpriteText JudgementText { get; private set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected TextJudgementPiece(HitResult result)
        {
            Result = result;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(JudgementText = CreateJudgementText());

            JudgementText.Colour = colours.ForHitResult(Result);
            JudgementText.Text = Result.GetDescription().ToUpperInvariant();
        }

        protected abstract SpriteText CreateJudgementText();
    }
}
