// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Judgements;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class Catcher : SkinReloadableDrawable
    {
        /// <summary>
        /// The default colour used to tint hyper-dash fruit, along with the moving catcher, its trail
        /// and end glow/after-image during a hyper-dash.
        /// </summary>
        public static readonly Color4 DEFAULT_HYPER_DASH_COLOUR = Color4.Red;

        /// <summary>
        /// The duration between transitioning to hyper-dash state.
        /// </summary>
        public const double HYPER_DASH_TRANSITION_DURATION = 180;

        /// <summary>
        /// Whether we are hyper-dashing or not.
        /// </summary>
        public bool HyperDashing => hyperDashModifier != 1;

        /// <summary>
        /// Whether <see cref="DrawablePalpableCatchHitObject"/> fruit should appear on the plate.
        /// </summary>
        public bool CatchFruitOnPlate
        {
            set => plate.PlaceCaughtObject.Value = value;
        }

        /// <summary>
        /// The relative space to cover in 1 millisecond. based on 1 game pixel per millisecond as in osu-stable.
        /// </summary>
        public const double BASE_SPEED = 1.0;

        /// <summary>
        /// The current speed of the catcher.
        /// </summary>
        public double Speed => (Dashing ? 1 : 0.5) * BASE_SPEED * hyperDashModifier;

        [NotNull]
        private readonly Container trailsTarget;

        private CatcherTrailDisplay trails;

        public CatcherAnimationState CurrentState
        {
            get => body.AnimationState.Value;
            private set => body.AnimationState.Value = value;
        }

        /// <summary>
        /// The width of the catcher which can receive fruit. Equivalent to "catchMargin" in osu-stable.
        /// </summary>
        public const float ALLOWED_CATCH_RANGE = 0.8f;

        private bool dashing;

        public bool Dashing
        {
            get => dashing;
            set
            {
                if (value == dashing) return;

                dashing = value;

                updateTrailVisibility();
            }
        }

        public Direction VisualDirection
        {
            get => Scale.X > 0 ? Direction.Right : Direction.Left;
            set => Scale = new Vector2((value == Direction.Right ? 1 : -1) * Math.Abs(Scale.X), Scale.Y);
        }

        /// <summary>
        /// Width of the area that can be used to attempt catches during gameplay.
        /// </summary>
        private readonly float catchWidth;

        private readonly SkinnableCatcher body;

        private readonly CatcherPlate plate;

        private Color4 hyperDashColour = DEFAULT_HYPER_DASH_COLOUR;
        private Color4 hyperDashEndGlowColour = DEFAULT_HYPER_DASH_COLOUR;

        private double hyperDashModifier = 1;
        private int hyperDashDirection;
        private float hyperDashTargetPosition;

        public Catcher([NotNull] Container trailsTarget, BeatmapDifficulty difficulty = null)
        {
            this.trailsTarget = trailsTarget;

            Origin = Anchor.TopCentre;

            Size = new Vector2(CatcherArea.CATCHER_SIZE);
            if (difficulty != null)
                Scale = calculateScale(difficulty);

            catchWidth = CalculateCatchWidth(Scale);

            InternalChildren = new Drawable[]
            {
                body = new SkinnableCatcher(),
                plate = new CatcherPlate
                {
                    Anchor = Anchor.TopCentre
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            plate.GenerateHitLighting.BindTo(config.GetBindable<bool>(OsuSetting.HitLighting));
            trails = new CatcherTrailDisplay(this);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // don't add in above load as we may potentially modify a parent in an unsafe manner.
            trailsTarget.Add(trails);
        }

        /// <summary>
        /// Creates proxied content to be displayed beneath hitobjects.
        /// </summary>
        public Drawable CreateProxiedContent() => plate.CreateBackgroundLayerProxy();

        /// <summary>
        /// Calculates the scale of the catcher based off the provided beatmap difficulty.
        /// </summary>
        private static Vector2 calculateScale(BeatmapDifficulty difficulty) => new Vector2(1.0f - 0.7f * (difficulty.CircleSize - 5) / 5);

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="scale">The scale of the catcher.</param>
        public static float CalculateCatchWidth(Vector2 scale) => CatcherArea.CATCHER_SIZE * Math.Abs(scale.X) * ALLOWED_CATCH_RANGE;

        /// <summary>
        /// Calculates the width of the area used for attempting catches in gameplay.
        /// </summary>
        /// <param name="difficulty">The beatmap difficulty.</param>
        public static float CalculateCatchWidth(BeatmapDifficulty difficulty) => CalculateCatchWidth(calculateScale(difficulty));

        /// <summary>
        /// Determine if this catcher can catch a <see cref="CatchHitObject"/> in the current position.
        /// </summary>
        public bool CanCatch(CatchHitObject hitObject)
        {
            if (!(hitObject is PalpableCatchHitObject fruit))
                return false;

            var halfCatchWidth = catchWidth * 0.5f;

            // this stuff wil disappear once we move fruit to non-relative coordinate space in the future.
            var catchObjectPosition = fruit.EffectiveX;
            var catcherPosition = Position.X;

            return catchObjectPosition >= catcherPosition - halfCatchWidth &&
                   catchObjectPosition <= catcherPosition + halfCatchWidth;
        }

        public void OnNewResult(DrawableCatchHitObject drawableObject, JudgementResult result)
        {
            var catchResult = (CatchJudgementResult)result;
            catchResult.CatcherAnimationState = CurrentState;
            catchResult.CatcherHyperDash = HyperDashing;

            if (!(drawableObject is DrawablePalpableCatchHitObject palpableObject)) return;

            var hitObject = palpableObject.HitObject;

            if (result.IsHit)
                plate.OnHitObjectCaught(palpableObject, X);

            // droplet doesn't affect the catcher state
            if (hitObject is TinyDroplet) return;

            if (result.IsHit && hitObject.HyperDash)
            {
                var target = hitObject.HyperDashTarget;
                var timeDifference = target.StartTime - hitObject.StartTime;
                double positionDifference = target.EffectiveX - X;
                var velocity = positionDifference / Math.Max(1.0, timeDifference - 1000.0 / 60.0);

                SetHyperDashState(Math.Abs(velocity), target.EffectiveX);
            }
            else
                SetHyperDashState();

            if (result.IsHit)
                CurrentState = hitObject.Kiai ? CatcherAnimationState.Kiai : CatcherAnimationState.Idle;
            else if (!(hitObject is Banana))
                CurrentState = CatcherAnimationState.Fail;
        }

        public void OnRevertResult(DrawableCatchHitObject drawableObject, JudgementResult result)
        {
            var catchResult = (CatchJudgementResult)result;

            CurrentState = catchResult.CatcherAnimationState;

            if (HyperDashing != catchResult.CatcherHyperDash)
            {
                if (catchResult.CatcherHyperDash)
                    SetHyperDashState(2);
                else
                    SetHyperDashState();
            }

            plate.OnRevertResult(drawableObject);
        }

        /// <summary>
        /// Set hyper-dash state.
        /// </summary>
        /// <param name="modifier">The speed multiplier. If this is less or equals to 1, this catcher will be non-hyper-dashing state.</param>
        /// <param name="targetPosition">When this catcher crosses this position, this catcher ends hyper-dashing.</param>
        public void SetHyperDashState(double modifier = 1, float targetPosition = -1)
        {
            var wasHyperDashing = HyperDashing;

            if (modifier <= 1 || X == targetPosition)
            {
                hyperDashModifier = 1;
                hyperDashDirection = 0;

                if (wasHyperDashing)
                    runHyperDashStateTransition(false);
            }
            else
            {
                hyperDashModifier = modifier;
                hyperDashDirection = Math.Sign(targetPosition - X);
                hyperDashTargetPosition = targetPosition;

                if (!wasHyperDashing)
                {
                    trails.DisplayEndGlow();
                    runHyperDashStateTransition(true);
                }
            }
        }

        /// <summary>
        /// Drop any fruit off the plate.
        /// </summary>
        public void Drop() => plate.DropAll(DropAnimation.Drop);

        /// <summary>
        /// Explode all fruit off the plate.
        /// </summary>
        public void Explode() => plate.DropAll(DropAnimation.Explode);

        private void runHyperDashStateTransition(bool hyperDashing)
        {
            updateTrailVisibility();

            this.FadeColour(hyperDashing ? hyperDashColour : Color4.White, HYPER_DASH_TRANSITION_DURATION, Easing.OutQuint);
        }

        private void updateTrailVisibility() => trails.DisplayTrail = Dashing || HyperDashing;

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            hyperDashColour =
                skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ??
                DEFAULT_HYPER_DASH_COLOUR;

            hyperDashEndGlowColour =
                skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashAfterImage)?.Value ??
                hyperDashColour;

            trails.HyperDashTrailsColour = hyperDashColour;
            trails.EndGlowSpritesColour = hyperDashEndGlowColour;

            runHyperDashStateTransition(HyperDashing);
        }

        protected override void Update()
        {
            base.Update();

            // Correct overshooting.
            if ((hyperDashDirection > 0 && hyperDashTargetPosition < X) ||
                (hyperDashDirection < 0 && hyperDashTargetPosition > X))
            {
                X = hyperDashTargetPosition;
                SetHyperDashState();
            }
        }
    }
}
