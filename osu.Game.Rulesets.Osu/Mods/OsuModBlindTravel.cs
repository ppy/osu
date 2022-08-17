// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics;
using osuTK;
using osu.Game.Screens.Play;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Framework.Bindables;
using osu.Game.Screens;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModBlindTravel : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor, IApplicableToPlayer, IApplicableToDrawableRuleset<OsuHitObject>

    {
        private const int last_zoom_combo = 200;
        private const int zoom_every_combo_amount = 100;
        private const int apply_zoom_duration = 1000;
        private const int default_follow_delay = 100;
        private const double default_zoom = 1.8;
        private const double zoom_with_combo_by = 0.1;

        public override string Name => "Blind Travel";
        public override string Acronym => "BT";
        public override IconUsage? Icon => FontAwesome.Solid.PlaneDeparture;
        public override ModType Type => ModType.Fun;
        public override string Description => "You cursor is focused.";
        public override double ScoreMultiplier => 1;


        [SettingSource("Focus delay", "Milliseconds for for your cursor be focused")]
        public BindableDouble CameraDelay { get; } = new BindableDouble(default_follow_delay)
        {
            MinValue = default_follow_delay,
            MaxValue = default_follow_delay * 10,
            Precision = default_follow_delay,
        };

        private double cameraDelay => CameraDelay.Value;


        [SettingSource("Base zoom", "Adjust the zoom applied to your cursor.")]
        public BindableDouble BaseZoom { get; } = new BindableDouble(default_zoom)
        {
            MinValue = 0.5f,
            MaxValue = 2,
            Precision = 0.05f
        };

        private double baseZoom => BaseZoom.Value;

        private double currentZoom;

        private double CurrentZoom
        {
            get => currentZoom;
            set
            {
                Debug.Assert(ParallaxContainer != null && Player != null);

                if (currentZoom == value) return;

                currentZoom = value;

                ParallaxContainer.ParallaxAmount = ParallaxContainer.DEFAULT_PARALLAX_AMOUNT * Player.BackgroundParallaxAmount * (float)value;
            }
        }

        [SettingSource("Change zoom based on combo", "Zooms in on your cursor based on combo")]
        public BindableBool ComboBasedZoom { get; } = new BindableBool(true);

        private OsuPlayfield? Playfield;

        private Player? Player;

        private ParallaxContainer? ParallaxContainer;

        private IFrameStableClock? GameplayClock;

        private Vector2 ParentHalfVector
        {
            get
            {
                Debug.Assert(Playfield != null);
                return Playfield.DrawSize / 2;
            }
        }

        private DateTime increasedVisibilityModeExpiration = DateTime.Now;

        protected BindableInt Combo = new BindableInt();

        private List<Drawable> ZoomedDrawables = new List<Drawable>();

        private List<Drawable> DrawablesFollowingCursor = new List<Drawable>();

        public void ApplyToPlayer(Player player)
        {
            Player = player;
            ParallaxContainer = player.FindClosestParent<OsuScreenStack>().parallaxContainer;
            CurrentZoom = baseZoom;
        }

        public void ApplyToDrawableRuleset(Rulesets.UI.DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            Playfield = (OsuPlayfield)drawableRuleset.Playfield;
            GameplayClock = drawableRuleset.FrameStableClock;

            ZoomedDrawables.Add(Playfield);
            DrawablesFollowingCursor.Add(Playfield);
        }

        public void Update(Playfield _)
        {
            Debug.Assert(ParallaxContainer != null && Playfield != null && GameplayClock != null && Player != null);

            var cursorPos = Playfield.Cursor.ActiveCursor.DrawPosition;

            // applies parallax to managed cursors (such as auto).
            ParallaxContainer.MousePosition = cursorPos;

            var translatedCursorPosition = -cursorPos;

            ApplyZoomForZoomedDrawables(CurrentZoom);
            MoveDrawablesFollowingCursor(translatedCursorPosition);
        }

        private void MoveDrawablesFollowingCursor(Vector2 position)
        {
            Debug.Assert(GameplayClock != null && Playfield != null);

            double dampLength = cameraDelay / 2;

            foreach (var drawable in DrawablesFollowingCursor)
            {
                var followPosition = Vector2.Clamp(position + ParentHalfVector, -ParentHalfVector, ParentHalfVector);

                float x = (float)Interpolation.DampContinuously(drawable.X, followPosition.X, dampLength, GameplayClock.ElapsedFrameTime);

                float y = (float)Interpolation.DampContinuously(drawable.Y, followPosition.Y, dampLength, GameplayClock.ElapsedFrameTime);

                // Handle playback edge cases (for whatever reason one of these values may be infinity)
                if (Double.IsInfinity(x) || Double.IsInfinity(y))
                    continue;

                drawable.Position = new Vector2(x, y);
            }
        }

        private void ApplyZoomForZoomedDrawables(double zoom)
        {
            Debug.Assert(GameplayClock != null);

            foreach (var drawable in ZoomedDrawables)
            {
                float currentScale = (float)Interpolation.ValueAt(Math.Min(Math.Abs(GameplayClock.ElapsedFrameTime), apply_zoom_duration), drawable.Scale.X, zoom, 0, apply_zoom_duration, Easing.Out);

                drawable.Scale = new Vector2(currentScale, currentScale);
            }
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (ComboBasedZoom.Value)
            {
                scoreProcessor.Combo.BindTo(Combo);
                Combo.ValueChanged += OnComboChange;
            }
        }

        private void OnComboChange(ValueChangedEvent<int> e)
        {
            CurrentZoom = GetZoomForCombo(e.NewValue);
        }

        private double GetZoomForCombo(int combo)
        {
            double setCombo = Math.Min(combo, last_zoom_combo);
            return baseZoom + zoom_with_combo_by * (int)Math.Floor(setCombo / zoom_every_combo_amount);
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
