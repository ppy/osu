// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinner : DrawableOsuHitObject
    {
        protected readonly Spinner Spinner;

        private readonly Container<DrawableSpinnerTick> ticks;

        public readonly SpinnerRotationTracker RotationTracker;
        public readonly SpinnerSpmCounter SpmCounter;
        private readonly SpinnerBonusDisplay bonusDisplay;

        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();

        public DrawableSpinner(Spinner s)
            : base(s)
        {
            Origin = Anchor.Centre;
            Position = s.Position;

            RelativeSizeAxes = Axes.Both;

            Spinner = s;

            InternalChildren = new Drawable[]
            {
                ticks = new Container<DrawableSpinnerTick>(),
                new AspectContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SpinnerBody), _ => new DefaultSpinnerDisc()),
                        RotationTracker = new SpinnerRotationTracker(Spinner)
                    }
                },
                SpmCounter = new SpinnerSpmCounter
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = 120,
                    Alpha = 0
                },
                bonusDisplay = new SpinnerBonusDisplay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = -120,
                }
            };
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableSpinnerTick tick:
                    ticks.Add(tick);
                    break;
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            using (BeginDelayedSequence(Spinner.Duration, true))
                this.FadeOut(160);
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            ticks.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SpinnerBonusTick bonusTick:
                    return new DrawableSpinnerBonusTick(bonusTick);

                case SpinnerTick tick:
                    return new DrawableSpinnerTick(tick);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            positionBindable.BindValueChanged(pos => Position = pos.NewValue);
            positionBindable.BindTo(HitObject.PositionBindable);
        }

        /// <summary>
        /// The completion progress of this spinner from 0..1 (clamped).
        /// </summary>
        public float Progress => Math.Clamp(RotationTracker.CumulativeRotation / 360 / Spinner.SpinsRequired, 0, 1);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            RotationTracker.Complete.Value = Progress >= 1;

            if (userTriggered || Time.Current < Spinner.EndTime)
                return;

            // Trigger a miss result for remaining ticks to avoid infinite gameplay.
            foreach (var tick in ticks.Where(t => !t.IsHit))
                tick.TriggerResult(false);

            ApplyResult(r =>
            {
                if (Progress >= 1)
                    r.Type = HitResult.Great;
                else if (Progress > .9)
                    r.Type = HitResult.Good;
                else if (Progress > .75)
                    r.Type = HitResult.Meh;
                else if (Time.Current >= Spinner.EndTime)
                    r.Type = HitResult.Miss;
            });
        }

        protected override void Update()
        {
            base.Update();
            if (HandleUserInput)
                RotationTracker.Tracking = OsuActionInputManager?.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton) ?? false;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!SpmCounter.IsPresent && RotationTracker.Tracking)
                SpmCounter.FadeIn(HitObject.TimeFadeIn);
            SpmCounter.SetRotation(RotationTracker.CumulativeRotation);

            updateBonusScore();
        }

        private int wholeSpins;

        private void updateBonusScore()
        {
            if (ticks.Count == 0)
                return;

            int spins = (int)(RotationTracker.CumulativeRotation / 360);

            if (spins < wholeSpins)
            {
                // rewinding, silently handle
                wholeSpins = spins;
                return;
            }

            while (wholeSpins != spins)
            {
                var tick = ticks.FirstOrDefault(t => !t.IsHit);

                // tick may be null if we've hit the spin limit.
                if (tick != null)
                {
                    tick.TriggerResult(true);
                    if (tick is DrawableSpinnerBonusTick)
                        bonusDisplay.SetBonusCount(spins - Spinner.SpinsRequired);
                }

                wholeSpins++;
            }
        }
    }
}
