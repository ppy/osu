// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using osuTK;
using osu.Game.Screens.Play;
using osu.Framework.Threading;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Framework.Bindables;
using osu.Game.Screens;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Graphics.Containers;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModBlindTravel : ModWithVisibilityAdjustment, IUpdatableByPlayfield, IApplicableToScoreProcessor, IApplicableToPlayer, IApplicableToDrawableRuleset<OsuHitObject>

    {
        public override string Name => "Blind Travel";
        public override string Acronym => "BT";
        public override IconUsage? Icon => FontAwesome.Solid.PlaneDeparture;
        public override ModType Type => ModType.Fun;
        public override string Description => "You cursor is focused.";
        public override double ScoreMultiplier => 1;

        private const float default_follow_delay = 100;

        [SettingSource("Camera delay", "Milliseconds for the camera to catch up with your cursor")]
        public BindableDouble CameraDelay { get; } = new BindableDouble(default_follow_delay)
        {
            MinValue = default_follow_delay,
            MaxValue = default_follow_delay * 10,
            Precision = default_follow_delay,
        };

        private double cameraDelay => CameraDelay.Value;

        private float applyIncreasedVisibilityDuration = 6000;

        private float increasedVisibilityDuration = 1000;

        private const float default_scope_scale = 1;

        private const float default_scope_fov = 0.6f;

        private bool increasedVisibilityMode;

        private Scheduler scheduler = new Scheduler();

        private bool isTraveling;

        [SettingSource("Base FOV", "Adjust the base field of view")]
        public BindableFloat BaseScopeFov { get; } = new BindableFloat(default_scope_fov)
        {
            MinValue = default_scope_fov,
            MaxValue = default_scope_scale,
            Precision = 0.05f
        };

        private float baseScopeFOV => BaseScopeFov.Value;

        private float currentScopeFOV = default_scope_fov;

        // scaling works as (if fov = 1; scale is half of that)
        private float CurrentScopeFovAsScale => 1 / currentScopeFOV;

        [SettingSource("Change FOV based on combo", "Shrinks the FOV based on combo")]
        public BindableBool ComboBasedFOV { get; } = new BindableBool(true);

        private const float shrinkFOVWithComboBy = 0.05f;

        private Playfield? playfield;

        private ParallaxContainer? parallaxContainer;

        private float baseParallaxAmount;

        private IFrameStableClock? gameplayClock;

        public void ApplyToPlayer(Player player)
        {
            baseParallaxAmount = player.BackgroundParallaxAmount;
            parallaxContainer = player.FindClosestParent<OsuScreenStack>().parallaxContainer;

            currentScopeFOV = baseScopeFOV;
            applyParallaxForFOV(parallaxContainer, currentScopeFOV);

            player.IsBreakTime.ValueChanged += onBreakTime;
        }

        private void onBreakTime(ValueChangedEvent<bool> e)
        {
            InternalApplyIncreasedVisibilityState();
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            playfield = drawableRuleset.Playfield;
            gameplayClock = drawableRuleset.FrameStableClock;
        }

        public void Update(Playfield playfield)
        {
            Debug.Assert(parallaxContainer != null);

            scheduler.Update();

            var cursorPos = playfield.Cursor.ActiveCursor.DrawPosition;

            // applies parallax to managed cursors (such as auto).
            parallaxContainer.MousePosition = cursorPos;

            if (!isTraveling)
                return;

            var scopePosition = -cursorPos;

            moveDrawableToScope(playfield, scopePosition, playfield);
        }

        private void moveDrawableToScope(Drawable drawable, Vector2 position, Playfield playfield)
        {
            Debug.Assert(gameplayClock != null);

            var scopePositionForDrawable = position + playfield.DrawSize / 2;

            double dampLength = cameraDelay / 2;

            scopePositionForDrawable.X = (float)Interpolation.DampContinuously(drawable.X, scopePositionForDrawable.X, dampLength, gameplayClock.ElapsedFrameTime);
            scopePositionForDrawable.Y = (float)Interpolation.DampContinuously(drawable.Y, scopePositionForDrawable.Y, dampLength, gameplayClock.ElapsedFrameTime);

            drawable.Position = scopePositionForDrawable;
        }


        private void InternalApplyIncreasedVisibilityState()
        {
            if (increasedVisibilityMode)
                return;

            Debug.Assert(playfield != null && parallaxContainer != null);

            increasedVisibilityMode = true;
            isTraveling = false;


            ApplyBlindTravel(playfield, default_scope_scale);
            applyParallaxForFOV(parallaxContainer, default_scope_scale);

            scheduler.AddDelayed(() => increasedVisibilityMode = false, increasedVisibilityDuration);
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            InternalApplyIncreasedVisibilityState();
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            Debug.Assert(playfield != null && parallaxContainer != null);
            if (!increasedVisibilityMode && !isTraveling)
            {
                ApplyBlindTravel(playfield, CurrentScopeFovAsScale);
                applyParallaxForFOV(parallaxContainer, currentScopeFOV);

                isTraveling = true;
            }
        }

        private void ApplyBlindTravel(Playfield playfield, float scale)
        {
            playfield.ScaleTo(scale, applyIncreasedVisibilityDuration, Easing.Out);
        }

        protected BindableInt Combo = new BindableInt();

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (ComboBasedFOV.Value)
            {
                scoreProcessor.Combo.BindTo(Combo);
                Combo.ValueChanged += OnComboChange;
            }
        }

        private const int lastShrinkableSizeCombo = 200;
        private const int shrinkEveryComboAmount = 100;

        private void applyParallaxForFOV(ParallaxContainer parallaxContainer, float fov)
        {
            // The lower the fov, the more shakier it gets
            parallaxContainer.ParallaxAmount = ParallaxContainer.DEFAULT_PARALLAX_AMOUNT * baseParallaxAmount * (25 / fov);
        }

        private void OnComboChange(ValueChangedEvent<int> e)
        {
            Debug.Assert(parallaxContainer != null);

            var combo = e.NewValue;
            if (combo % shrinkEveryComboAmount == 0)
            {
                currentScopeFOV = getScopeFOVForCombo(combo);
                applyParallaxForFOV(parallaxContainer, currentScopeFOV);
            }
        }

        private float getScopeFOVForCombo(int combo)
        {
            var setCombo = Math.Min(combo, lastShrinkableSizeCombo);
            return baseScopeFOV - shrinkFOVWithComboBy * (setCombo / shrinkEveryComboAmount);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            switch (rank)
            {
                case ScoreRank.X:
                    return ScoreRank.XH;

                case ScoreRank.S:
                    return ScoreRank.SH;

                default:
                    return rank;
            }
        }

    }
}
