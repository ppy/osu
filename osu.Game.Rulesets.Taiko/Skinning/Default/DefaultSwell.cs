// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Default
{
    public partial class DefaultSwell : Container
    {
        private const float target_ring_thick_border = 1.4f;
        private const float target_ring_thin_border = 1f;
        private const float target_ring_scale = 5f;
        private const float inner_ring_alpha = 0.65f;

        private DrawableSwell drawableSwell = null!;

        private readonly Container bodyContainer;
        private readonly CircularContainer targetRing;
        private readonly CircularContainer expandingRing;
        private readonly Drawable centreCircle;
        private int numHits;

        public DefaultSwell()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            Content.Add(bodyContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Depth = 1,
                Children = new[]
                {
                    expandingRing = new CircularContainer
                    {
                        Name = "Expanding ring",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                        Blending = BlendingParameters.Additive,
                        Masking = true,
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = inner_ring_alpha,
                            }
                        }
                    },
                    targetRing = new CircularContainer
                    {
                        Name = "Target ring (thick border)",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderThickness = target_ring_thick_border,
                        Blending = BlendingParameters.Additive,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true
                            },
                            new CircularContainer
                            {
                                Name = "Target ring (thin border)",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                BorderThickness = target_ring_thin_border,
                                BorderColour = Color4.White,
                                Children = new[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0,
                                        AlwaysPresent = true
                                    }
                                }
                            }
                        }
                    },
                    centreCircle = CreateCentreCircle(),
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject hitObject, OsuColour colours)
        {
            drawableSwell = (DrawableSwell)hitObject;
            drawableSwell.UpdateHitProgress += animateSwellProgress;
            drawableSwell.ApplyCustomUpdateState += updateStateTransforms;

            expandingRing.Colour = colours.YellowLight;
            targetRing.BorderColour = colours.YellowDark.Opacity(0.25f);
        }

        protected virtual Drawable CreateCentreCircle()
        {
            return new SwellCirclePiece
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        private void animateSwellProgress(int numHits)
        {
            this.numHits = numHits;

            float completion = (float)numHits / drawableSwell.HitObject.RequiredHits;
            expandingRing.Alpha += Math.Clamp(completion / 16, 0.1f, 0.6f);
        }

        protected override void Update()
        {
            base.Update();

            float completion = (float)numHits / drawableSwell.HitObject.RequiredHits;

            centreCircle.Rotation = (float)Interpolation.DampContinuously(centreCircle.Rotation,
                (float)(completion * drawableSwell.HitObject.Duration / 8), 500, Math.Abs(Time.Elapsed));
            expandingRing.Scale = new Vector2((float)Interpolation.DampContinuously(expandingRing.Scale.X,
                1f + Math.Min(target_ring_scale - 1f, (target_ring_scale - 1f) * completion * 1.3f), 35, Math.Abs(Time.Elapsed)));
            expandingRing.Alpha = (float)Interpolation.DampContinuously(expandingRing.Alpha, completion / 16, 250, Math.Abs(Time.Elapsed));
        }

        private void updateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            if (!(drawableHitObject is DrawableSwell))
                return;

            Swell swell = drawableSwell.HitObject;

            using (BeginAbsoluteSequence(swell.StartTime))
            {
                if (state == ArmedState.Idle)
                    expandingRing.FadeTo(0);

                const double ring_appear_offset = 100;

                targetRing.Delay(ring_appear_offset).ScaleTo(target_ring_scale, 400, Easing.OutQuint);
            }

            using (BeginAbsoluteSequence(drawableSwell.HitStateUpdateTime))
            {
                const double transition_duration = 300;

                bodyContainer.FadeOut(transition_duration, Easing.OutQuad);
                bodyContainer.ScaleTo(1.4f, transition_duration);
                centreCircle.FadeOut(transition_duration, Easing.OutQuad);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableSwell.IsNotNull())
            {
                drawableSwell.UpdateHitProgress -= animateSwellProgress;
                drawableSwell.ApplyCustomUpdateState -= updateStateTransforms;
            }
        }
    }
}
