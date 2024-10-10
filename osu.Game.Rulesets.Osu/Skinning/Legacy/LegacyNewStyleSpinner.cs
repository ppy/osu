// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    /// <summary>
    /// Legacy skinned spinner with two main spinning layers, one fixed overlay and one final spinning overlay.
    /// No background layer.
    /// </summary>
    public partial class LegacyNewStyleSpinner : LegacySpinner
    {
        private Sprite glow = null!;
        private Sprite discBottom = null!;
        private Sprite discTop = null!;
        private Sprite spinningMiddle = null!;
        private Sprite fixedMiddle = null!;

        private readonly Color4 glowColour = new Color4(3, 151, 255, 255);

        private Container scaleContainer = null!;

        [BackgroundDependencyLoader]
        private void load(ISkinSource source)
        {
            AddInternal(scaleContainer = new Container
            {
                Scale = new Vector2(SPRITE_SCALE),
                Anchor = Anchor.TopCentre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Y = SPINNER_Y_CENTRE,
                Children = new Drawable[]
                {
                    glow = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = source.GetTexture("spinner-glow"),
                        Blending = BlendingParameters.Additive,
                        Colour = glowColour,
                    },
                    discBottom = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = source.GetTexture("spinner-bottom"),
                    },
                    discTop = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = source.GetTexture("spinner-top"),
                    },
                    fixedMiddle = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = source.GetTexture("spinner-middle"),
                    },
                    spinningMiddle = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = source.GetTexture("spinner-middle2"),
                    },
                }
            });

            var topProvider = source.FindProvider(s => s.GetTexture("spinner-top") != null);

            if (topProvider is ISkinTransformer transformer && !(transformer.Skin is DefaultLegacySkin))
            {
                AddInternal(ApproachCircle = new Sprite
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    Texture = source.GetTexture("spinner-approachcircle"),
                    Scale = new Vector2(SPRITE_SCALE * 1.86f),
                    Y = SPINNER_Y_CENTRE,
                });
            }
        }

        protected override void UpdateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            base.UpdateStateTransforms(drawableHitObject, state);

            switch (drawableHitObject)
            {
                case DrawableSpinner d:
                    Spinner spinner = d.HitObject;

                    using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimePreempt))
                        this.FadeOut();

                    using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimeFadeIn))
                        this.FadeInFromZero(spinner.TimeFadeIn);

                    using (BeginAbsoluteSequence(spinner.StartTime - spinner.TimePreempt))
                    {
                        fixedMiddle.FadeColour(Color4.White);

                        using (BeginDelayedSequence(spinner.TimePreempt))
                            fixedMiddle.FadeColour(Color4.Red, spinner.Duration);
                    }

                    if (state == ArmedState.Hit)
                    {
                        using (BeginAbsoluteSequence(d.HitStateUpdateTime))
                            glow.FadeOut(300);
                    }

                    break;

                case DrawableSpinnerBonusTick:
                    if (state == ArmedState.Hit)
                        glow.FlashColour(Color4.White, 200);

                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            float turnRatio = spinningMiddle.Texture != null ? 0.5f : 1;
            discTop.Rotation = DrawableSpinner.RotationTracker.Rotation * turnRatio;
            spinningMiddle.Rotation = DrawableSpinner.RotationTracker.Rotation;

            discBottom.Rotation = discTop.Rotation / 3;

            glow.Alpha = DrawableSpinner.Progress;

            scaleContainer.Scale = new Vector2(SPRITE_SCALE * (0.8f + (float)Interpolation.ApplyEasing(Easing.Out, DrawableSpinner.Progress) * 0.2f));
        }
    }
}
