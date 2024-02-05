// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class DefaultJudgementPieceSliderTickMiss : CompositeDrawable, IAnimatableJudgement
    {
        private readonly HitResult result;
        private Circle piece = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public DefaultJudgementPieceSliderTickMiss(HitResult result)
        {
            this.result = result;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(piece = new Circle
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Blending = BlendingParameters.Additive,
                Colour = colours.ForHitResult(result),
                Size = new Vector2(DrawableSliderTick.DEFAULT_TICK_SIZE)
            });
        }

        public void PlayAnimation()
        {
            this.ScaleTo(1.4f);
            this.ScaleTo(1f, 150, Easing.Out);

            this.FadeOutFromOne(600);
        }

        public Drawable? GetAboveHitObjectsProxiedContent() => piece.CreateProxy();
    }
}
