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
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Framework.Utils;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModZoomed : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor, IApplicableToPlayer, IApplicableToDrawableRuleset<OsuHitObject>
    {
        private const int last_zoom_combo = 200;
        private const int zoom_every_combo_amount = 100;
        private const int apply_zoom_duration = 1000;
        private const int default_follow_delay = 0;
        private const double default_zoom = 1.8;
        private const double zoom_with_combo_by = 0.1;

        public override string Name => "Zoomed";
        public override string Acronym => "ZM";
        public override IconUsage? Icon => FontAwesome.Solid.Glasses;
        public override ModType Type => ModType.Fun;
        public override string Description => "Big brother is watching your cursor.";
        public override double ScoreMultiplier => 1;

        [SettingSource("Focus delay", "Milliseconds for for your cursor be focused")]
        public BindableInt CameraDelay { get; } = new BindableInt(default_follow_delay)
        {
            MinValue = default_follow_delay,
            MaxValue = 1000,
            Precision = 100,
        };

        private int cameraDelay => CameraDelay.Value;

        [SettingSource("Base zoom", "Adjust the zoom applied to your cursor.")]
        public BindableDouble BaseZoom { get; } = new BindableDouble(default_zoom)
        {
            MinValue = 1.5,
            MaxValue = 2,
            Precision = 0.05
        };

        private double baseZoom => BaseZoom.Value;

        private double currentZoom;

        [SettingSource("Change zoom based on combo", "Zooms in on your cursor based on combo")]
        public BindableBool ComboBasedZoom { get; } = new BindableBool(true);

        private BindableInt combo = new BindableInt();

        private OsuPlayfield? playfield;

        private Player? player;

        private IFrameStableClock? gameplayClock;

        private readonly List<Drawable> zoomedDrawables = new List<Drawable>();

        private readonly List<Drawable> drawablesFollowingCursor = new List<Drawable>();

        public void ApplyToPlayer(Player player)
        {
            this.player = player;
            currentZoom = baseZoom;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            playfield = (OsuPlayfield)drawableRuleset.Playfield;
            gameplayClock = drawableRuleset.FrameStableClock;

            zoomedDrawables.Add(playfield);
            drawablesFollowingCursor.Add(playfield);
        }

        public void Update(Playfield _)
        {
            applyZoomForZoomedDrawables();
            moveDrawablesFollowingCursor();
        }

        private Vector2 getDrawablePositionForCursorPosition(Drawable drawable)
        {
            Debug.Assert(playfield != null);

            var position = playfield.Cursor.ActiveCursor.DrawPosition;

            return Vector2.Clamp(playfield.OriginPosition - position, -playfield.OriginPosition, playfield.OriginPosition);
        }

        private void moveDrawablesFollowingCursor()
        {
            Debug.Assert(gameplayClock != null);

            // prevent division by 0
            if (Precision.AlmostEquals(cameraDelay, 0))
            {
                foreach (var drawable in drawablesFollowingCursor)
                    drawable.Position = getDrawablePositionForCursorPosition(drawable);

                return;
            }

            foreach (var drawable in drawablesFollowingCursor)
            {
                var followPosition = getDrawablePositionForCursorPosition(drawable);

                drawable.Position = Interpolation.ValueAt(
                    Math.Min(Math.Abs(gameplayClock.ElapsedFrameTime), cameraDelay), drawable.Position, followPosition, 0, cameraDelay, Easing.Out);
            }
        }

        private void applyZoomForZoomedDrawables()
        {
            Debug.Assert(gameplayClock != null);

            foreach (var drawable in zoomedDrawables)
            {
                double currentScale = Interpolation.ValueAt(Math.Min(Math.Abs(gameplayClock.ElapsedFrameTime), apply_zoom_duration), drawable.Scale.X, currentZoom, 0, apply_zoom_duration, Easing.Out);

                drawable.Scale = new Vector2((float)currentScale);
            }
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (ComboBasedZoom.Value)
            {
                scoreProcessor.Combo.BindTo(combo);
                combo.ValueChanged += onComboChange;
            }
        }

        private void onComboChange(ValueChangedEvent<int> e)
        {
            currentZoom = getZoomForCombo(e.NewValue);
        }

        private double getZoomForCombo(int combo)
        {
            double setCombo = Math.Min(combo, last_zoom_combo);
            return baseZoom + zoom_with_combo_by * Math.Floor(setCombo / zoom_every_combo_amount);
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
