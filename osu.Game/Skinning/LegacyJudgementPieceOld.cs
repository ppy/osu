// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Skinning
{
    public partial class LegacyJudgementPieceOld : CompositeDrawable, IAnimatableJudgement
    {
        private readonly HitResult result;

        private readonly float finalScale;
        private readonly bool forceTransforms;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        public LegacyJudgementPieceOld(HitResult result, Func<Drawable> createMainDrawable, float finalScale = 1f, bool forceTransforms = false)
        {
            this.result = result;
            this.finalScale = finalScale;
            this.forceTransforms = forceTransforms;

            AutoSizeAxes = Axes.Both;
            Origin = Anchor.Centre;

            InternalChild = createMainDrawable();
        }

        public virtual void PlayAnimation()
        {
            var animation = InternalChild as IFramedAnimation;

            animation?.GotoFrame(0);

            const double fade_in_length = 120;
            const double fade_out_delay = 500;
            const double fade_out_length = 600;

            this.FadeInFromZero(fade_in_length);
            this.Delay(fade_out_delay).FadeOut(fade_out_length);

            // legacy judgements don't play any transforms if they are an animation.... UNLESS they are the temporary displayed judgement from new piece.
            if (animation?.FrameCount > 1 && !forceTransforms)
                return;

            switch (result)
            {
                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    decimal? legacyVersion = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value;

                    if (legacyVersion >= 2.0m)
                    {
                        this.MoveTo(new Vector2(0, -5));
                        this.MoveToOffset(new Vector2(0, 80), fade_out_delay + fade_out_length, Easing.In);
                    }

                    float rotation = RNG.NextSingle(-8.6f, 8.6f);

                    this.RotateTo(0);
                    this.RotateTo(rotation, fade_in_length)
                        .Then().RotateTo(rotation * 2, fade_out_delay + fade_out_length - fade_in_length, Easing.In);
                    break;

                default:

                    this.ScaleTo(0.6f).Then()
                        .ScaleTo(1.1f, fade_in_length * 0.8f).Then() // t = 0.8
                        .Delay(fade_in_length * 0.2f) // t = 1.0
                        .ScaleTo(0.9f, fade_in_length * 0.2f).Then() // t = 1.2

                        // stable dictates scale of 0.9->1 over time 1.0 to 1.4, but we are already at 1.2.
                        // so we need to force the current value to be correct at 1.2 (0.95) then complete the
                        // second half of the transform.
                        .ScaleTo(0.95f).ScaleTo(finalScale, fade_in_length * 0.2f); // t = 1.4
                    break;
            }
        }

        public Drawable GetAboveHitObjectsProxiedContent() => CreateProxy();
    }
}
