// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public partial class DrawableSwell : DrawableTaikoHitObject<Swell>
    {
        /// <summary>
        /// Offset away from the start time of the swell at which the ring starts appearing.
        /// </summary>
        private const double ring_appear_offset = 100;

        private readonly SkinnableDrawable spinnerBody;
        private readonly Container<DrawableSwellTick> ticks;

        public override bool DisplayResult => false;

        public DrawableSwell()
            : this(null)
        {
        }

        public DrawableSwell([CanBeNull] Swell swell)
            : base(swell)
        {
            FillMode = FillMode.Fit;

            Content.Add(spinnerBody = new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.SwellBody), _ => new DefaultSwell())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            });

            AddInternal(ticks = new Container<DrawableSwellTick> { RelativeSizeAxes = Axes.Both });
        }

        protected override SkinnableDrawable CreateMainPiece() => new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.SwellCirclePiece),
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

                int numHits = ticks.Count(r => r.IsHit);

                (spinnerBody.Drawable as ISkinnableSwell)?.AnimateSwellProgress(this, numHits, MainPiece);

                if (numHits == HitObject.RequiredHits)
                    ApplyResult(r => r.Type = r.Judgement.MaxResult);
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

                ApplyResult(r => r.Type = numHits == HitObject.RequiredHits ? r.Judgement.MaxResult : r.Judgement.MinResult);
            }
        }

        protected override void UpdateStartTimeStateTransforms()
        {
            base.UpdateStartTimeStateTransforms();

            (spinnerBody.Drawable as ISkinnableSwell)?.AnimateSwellStart(this, MainPiece);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Idle:
                    // Only for rewind support. Reallows user inputs if swell is rewound from being hit/missed to being idle.
                    HandleUserInput = true;
                    break;

                case ArmedState.Miss:
                case ArmedState.Hit:
                    const int clear_animation_duration = 1200;

                    // Postpone drawable hitobject expiration until it has animated/faded out. Inputs on the object are disallowed during this delay.
                    LifetimeEnd = Time.Current + clear_animation_duration;
                    HandleUserInput = false;

                    (spinnerBody.Drawable as ISkinnableSwell)?.AnimateSwellCompletion(state, MainPiece);
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

        public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
        {
            // Don't handle keys before the swell starts
            if (Time.Current < HitObject.StartTime)
                return false;

            bool isCentre = e.Action == TaikoAction.LeftCentre || e.Action == TaikoAction.RightCentre;

            // Ensure alternating centre and rim hits
            if (lastWasCentre == isCentre)
                return false;

            lastWasCentre = isCentre;

            UpdateResult(true);

            return true;
        }
    }
}
