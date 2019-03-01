// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableSwell : DrawableTaikoHitObject<Swell>
    {
        private const float target_ring_thick_border = 1.4f;
        private const float target_ring_thin_border = 1f;
        private const float target_ring_scale = 5f;
        private const float inner_ring_alpha = 0.65f;

        private readonly List<DrawableSwellTick> ticks = new List<DrawableSwellTick>();

        private readonly Container bodyContainer;
        private readonly CircularContainer targetRing;
        private readonly CircularContainer expandingRing;

        private readonly SwellSymbolPiece symbol;

        public DrawableSwell(Swell swell)
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
                        Blending = BlendingMode.Additive,
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
                        Blending = BlendingMode.Additive,
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

            MainPiece.Add(symbol = new SwellSymbolPiece());

            foreach (var tick in HitObject.NestedHitObjects.OfType<SwellTick>())
            {
                var vis = new DrawableSwellTick(tick);

                ticks.Add(vis);
                AddInternal(vis);
                AddNested(vis);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            MainPiece.AccentColour = colours.YellowDark;
            expandingRing.Colour = colours.YellowLight;
            targetRing.BorderColour = colours.YellowDark.Opacity(0.25f);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // We need to set this here because RelativeSizeAxes won't/can't set our size by default with a different RelativeChildSize
            Width *= Parent.RelativeChildSize.X;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered)
            {
                var nextTick = ticks.Find(j => !j.IsHit);

                nextTick?.TriggerResult(HitResult.Great);

                var numHits = ticks.Count(r => r.IsHit);

                var completion = (float)numHits / HitObject.RequiredHits;

                expandingRing
                    .FadeTo(expandingRing.Alpha + MathHelper.Clamp(completion / 16, 0.1f, 0.6f), 50)
                    .Then()
                    .FadeTo(completion / 8, 2000, Easing.OutQuint);

                symbol.RotateTo((float)(completion * HitObject.Duration / 8), 4000, Easing.OutQuint);

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

                    tick.TriggerResult(HitResult.Miss);
                }

                var hitResult = numHits > HitObject.RequiredHits / 2 ? HitResult.Good : HitResult.Miss;

                ApplyResult(r => r.Type = hitResult);
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            const float preempt = 100;
            const float out_transition_time = 300;

            switch (state)
            {
                case ArmedState.Idle:
                    UnproxyContent();
                    expandingRing.FadeTo(0);
                    using (BeginAbsoluteSequence(HitObject.StartTime - preempt, true))
                        targetRing.ScaleTo(target_ring_scale, preempt * 4, Easing.OutQuint);
                    break;
                case ArmedState.Miss:
                case ArmedState.Hit:
                    this.FadeOut(out_transition_time, Easing.Out);
                    bodyContainer.ScaleTo(1.4f, out_transition_time);

                    Expire();
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            Size = BaseSize * Parent.RelativeChildSize;

            // Make the swell stop at the hit target
            X = Math.Max(0, X);

            double t = Math.Min(HitObject.StartTime, Time.Current);
            if (t == HitObject.StartTime)
                ProxyContent();
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
