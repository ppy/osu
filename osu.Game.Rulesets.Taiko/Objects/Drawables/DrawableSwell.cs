// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableSwell : DrawableTaikoHitObject<Swell>
    {
        private const float target_ring_thick_border = 1.4f;
        private const float target_ring_thin_border = 1f;
        private const float target_ring_scale = 5f;
        private const float inner_ring_alpha = 0.65f;

        /// <summary>
        /// Offset away from the start time of the swell at which the ring starts appearing.
        /// </summary>
        private const double ring_appear_offset = 100;

        private readonly Container<DrawableSwellTick> ticks;
        private readonly Container bodyContainer;
        private readonly CircularContainer targetRing;
        private readonly CircularContainer expandingRing;

        public DrawableSwell()
            : this(null)
        {
        }

        public DrawableSwell([CanBeNull] Swell swell)
            : base(swell)
        {
            FillMode = FillMode.Fit;

            Content.Add(bodyContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Depth = 1,
                Children = new Drawable[]
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
                    }
                }
            });

            AddInternal(ticks = new Container<DrawableSwellTick> { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            expandingRing.Colour = colours.YellowLight;
            targetRing.BorderColour = colours.YellowDark.Opacity(0.25f);
        }

        protected override SkinnableDrawable CreateMainPiece() => new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.Swell),
            _ => new SwellCirclePiece
            {
                // to allow for rotation transform
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

        protected override void OnFree()
        {
            base.OnFree();

            UnproxyContent();

            lastWasCentre = null;
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableSwellTick tick:
                    ticks.Add(tick);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            ticks.Clear(false);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SwellTick tick:
                    return new DrawableSwellTick(tick);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered)
            {
                DrawableSwellTick nextTick = null;

                foreach (var t in ticks)
                {
                    if (!t.Result.HasResult)
                    {
                        nextTick = t;
                        break;
                    }
                }

                nextTick?.TriggerResult(true);

                var numHits = ticks.Count(r => r.IsHit);

                var completion = (float)numHits / HitObject.RequiredHits;

                expandingRing
                    .FadeTo(expandingRing.Alpha + Math.Clamp(completion / 16, 0.1f, 0.6f), 50)
                    .Then()
                    .FadeTo(completion / 8, 2000, Easing.OutQuint);

                MainPiece.Drawable.RotateTo((float)(completion * HitObject.Duration / 8), 4000, Easing.OutQuint);

                expandingRing.ScaleTo(1f + Math.Min(target_ring_scale - 1f, (target_ring_scale - 1f) * completion * 1.3f), 260, Easing.OutQuint);

                if (numHits == HitObject.RequiredHits)
                    ApplyResult(r => r.Type = HitResult.Great);
            }
            else
            {
                if (timeOffset < 0)
                    return;

                int numHits = 0;

                foreach (var tick in ticks)
                {
                    if (tick.IsHit)
                    {
                        numHits++;
                        continue;
                    }

                    if (!tick.Result.HasResult)
                        tick.TriggerResult(false);
                }

                ApplyResult(r => r.Type = numHits > HitObject.RequiredHits / 2 ? HitResult.Ok : r.Judgement.MinResult);
            }
        }

        protected override void UpdateStartTimeStateTransforms()
        {
            base.UpdateStartTimeStateTransforms();

            using (BeginDelayedSequence(-ring_appear_offset, true))
                targetRing.ScaleTo(target_ring_scale, 400, Easing.OutQuint);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            const double transition_duration = 300;

            switch (state)
            {
                case ArmedState.Idle:
                    expandingRing.FadeTo(0);
                    break;

                case ArmedState.Miss:
                case ArmedState.Hit:
                    this.FadeOut(transition_duration, Easing.Out);
                    bodyContainer.ScaleTo(1.4f, transition_duration);
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            Size = BaseSize * Parent.RelativeChildSize;

            // Make the swell stop at the hit target
            X = Math.Max(0, X);

            if (Time.Current >= HitObject.StartTime - ring_appear_offset)
                ProxyContent();
            else
                UnproxyContent();
        }

        private bool? lastWasCentre;

        public override bool OnPressed(TaikoAction action)
        {
            // Don't handle keys before the swell starts
            if (Time.Current < HitObject.StartTime)
                return false;

            var isCentre = action == TaikoAction.LeftCentre || action == TaikoAction.RightCentre;

            // Ensure alternating centre and rim hits
            if (lastWasCentre == isCentre)
                return false;

            lastWasCentre = isCentre;

            UpdateResult(true);

            return true;
        }
    }
}
