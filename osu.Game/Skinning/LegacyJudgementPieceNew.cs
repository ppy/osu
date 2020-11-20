// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacyJudgementPieceNew : CompositeDrawable, IAnimatableJudgement
    {
        private readonly HitResult result;

        private readonly LegacyJudgementPieceOld temporaryOldStyle;

        private readonly Drawable mainPiece;

        private readonly ParticleExplosion particles;

        public LegacyJudgementPieceNew(HitResult result, Func<Drawable> createMainDrawable, Texture particleTexture)
        {
            this.result = result;

            AutoSizeAxes = Axes.Both;
            Origin = Anchor.Centre;

            InternalChildren = new[]
            {
                mainPiece = createMainDrawable().With(d =>
                {
                    d.Anchor = Anchor.Centre;
                    d.Origin = Anchor.Centre;
                })
            };

            if (particleTexture != null)
            {
                AddInternal(particles = new ParticleExplosion(particleTexture, 150, 1600)
                {
                    Size = new Vector2(140),
                    Depth = float.MaxValue,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }

            if (result != HitResult.Miss)
            {
                //new judgement shows old as a temporary effect
                AddInternal(temporaryOldStyle = new LegacyJudgementPieceOld(result, createMainDrawable, 1.05f)
                {
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }
        }

        public void PlayAnimation()
        {
            var animation = mainPiece as IFramedAnimation;

            animation?.GotoFrame(0);

            if (particles != null)
            {
                // start the particles already some way into their animation to break cluster away from centre.
                using (particles.BeginDelayedSequence(-100, true))
                    particles.Restart();
            }

            const double fade_in_length = 120;
            const double fade_out_delay = 500;
            const double fade_out_length = 600;

            this.FadeInFromZero(fade_in_length);
            this.Delay(fade_out_delay).FadeOut(fade_out_length);

            // new style non-miss judgements show the original style temporarily, with additive colour.
            if (temporaryOldStyle != null)
            {
                temporaryOldStyle.PlayAnimation();

                temporaryOldStyle.Hide();
                temporaryOldStyle.Delay(-16)
                                 .FadeTo(0.5f, 56, Easing.Out).Then()
                                 .FadeOut(300);
            }

            // legacy judgements don't play any transforms if they are an animation.
            if (animation?.FrameCount > 1)
                return;

            switch (result)
            {
                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    //todo: this only applies to osu! ruleset apparently.
                    this.MoveTo(new Vector2(0, -2));
                    this.MoveToOffset(new Vector2(0, 20), fade_out_delay + fade_out_length, Easing.In);

                    float rotation = RNG.NextSingle(-8.6f, 8.6f);

                    this.RotateTo(0);
                    this.RotateTo(rotation, fade_in_length)
                        .Then().RotateTo(rotation * 2, fade_out_delay + fade_out_length - fade_in_length, Easing.In);
                    break;

                default:
                    mainPiece.ScaleTo(0.9f);
                    mainPiece.ScaleTo(1.05f, fade_out_delay + fade_out_length);
                    break;
            }
        }

        public Drawable GetAboveHitObjectsProxiedContent() => temporaryOldStyle?.CreateProxy(); // for new style judgements, only the old style temporary display is in front of objects.
    }
}
